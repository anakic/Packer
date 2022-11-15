using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using System.ComponentModel.DataAnnotations;

namespace Packer2.Library.MinifiedQueryParser
{
    public class QueryParser
    {
        private readonly ILogger logger;
        private readonly ParserResultValidator validator;

        public class ParserResultValidator
        {
            private readonly ILogger logger;

            public ParserResultValidator(ILogger logger)
            {
                this.logger = logger;
            }

            public void ValidateExpression(QueryExpression expression, bool standalone)
            {
                var errorContext = new ErrorTrackingContext(logger);
                var expressionValidator = new FixedQueryExpressionValidator(errorContext);

                if (standalone)
                    expressionValidator.ValidateStandaloneExpression(expression);
                else
                    expressionValidator.ValidateExpression(expression);

                if (errorContext.HasError)
                    throw new FormatException($"Expression validation failed: {expression.ToString()}");
            }

            public void ValidateQuery(QueryDefinition definition)
            {
                var errorContext = new ErrorTrackingContext(logger);
                var expressionValidator = new FixedQueryExpressionValidator(errorContext);
                var queryValidator = new QueryDefinitionValidator(expressionValidator);

                queryValidator.Visit(errorContext, definition);
                
                if (errorContext.HasError)
                    throw new FormatException("Parsing query failed");

            }

            public void ValidateFilter(FilterDefinition filter)
            {
                var errorContext = new ErrorTrackingContext(logger);
                var expressionValidator = new FixedQueryExpressionValidator(errorContext);
                var queryValidator = new QueryDefinitionValidator(expressionValidator);

                queryValidator.Visit(errorContext, filter);

                if (errorContext.HasError)
                    throw new FormatException("Parsing query failed");
            }

            class ErrorTrackingContext : IErrorContext
            {
                private bool _hasError;
                private readonly ILogger logger;

                public bool HasError => _hasError;

                public ErrorTrackingContext(ILogger logger)
                {
                    this.logger = logger;
                }

                public void RegisterError(string messageTemplate, params object[] args)
                {
                    logger.LogError(messageTemplate, args);
                    _hasError = true;
                }

                public void RegisterWarning(string messageTemplate, params object[] args)
                {
                    logger.LogWarning(messageTemplate, args);
                }
            }

            class FixedQueryExpressionValidator : QueryExpressionValidator
            {
                public FixedQueryExpressionValidator(IErrorContext errorContext)
                    : base(errorContext)
                {
                }

                protected override void Visit(QueryPropertyExpression expression)
                {
                    if (expression.Expression.Subquery != null)
                    {
                        // The validator seems to be firing a false positive when the expression inside a property referes to a subquery.
                        // Might be good to check if this is still the case in future updates to PowerBI

                        // example query that manifests this bug:
                        /*
                         * min({
        from d in [dbth Ward]
        orderby d.TypeKind ascending
        select d.TypeKind }.[dbth Ward.TypeKind])
                         */
                        return;
                    }


                    base.Visit(expression);
                }
            }
        }

        public QueryParser(ILogger logger)
        {
            this.logger = logger;
            validator = new ParserResultValidator(logger);
        }

        public QueryDefinition ParseQuery(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.query();

            var queryDefinition = new QueryConstructorVisitor(validator).Visit(tree);
            validator.ValidateQuery(queryDefinition);
            
            return queryDefinition;
        }

        public FilterDefinition ParseFilter(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.query();
            var queryDefinition = new QueryConstructorVisitor(validator).Visit(tree);

            if (queryDefinition.Select?.Count > 0 && queryDefinition.Parameters?.Count > 0 || queryDefinition.Transform?.Count > 0 || queryDefinition.GroupBy?.Count > 0 || queryDefinition.Let?.Count > 0 || queryDefinition.OrderBy?.Count > 0 || queryDefinition.Skip != null || queryDefinition.Top != null)
                throw new FormatException("Was expecting filter but got query");

            var filter = new FilterDefinition()
            {
                From = queryDefinition.From,
                Where = queryDefinition.Where
            };

            validator.ValidateFilter(filter);

            if(input != filter.ToString())
                throw new FormatException($"Parsing filter did not throw an exception but the constructed filter does not match the input string. Input string was '{input}', while the minimized constructed filter was '{filter}'!");

            return filter;
        }

        public QueryExpression ParseExpression(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.expressionContainer();

            var expression = new QueryExpressionVisitor(validator, true).VisitValidated(tree);

            if (input != expression.ToString())
                throw new FormatException($"Parsing expression did not throw an exception but the constructed expression does not match the input string. Input string was '{input}', while the minimized constructed expression was '{expression}'!");

            return expression;
        }

        private pbiqParser CreateParser(string input)
        {
            var lexer = new pbiqLexer(CharStreams.fromString(input));
            return new pbiqParser(new CommonTokenStream(lexer));
        }

        class QueryConstructorVisitor : pbiqParserBaseVisitor<QueryDefinition>
        {
            private readonly ParserResultValidator validator;

            public QueryConstructorVisitor(ParserResultValidator validator)
            {
                this.validator = validator;
            }

            public override QueryDefinition VisitQuery([NotNull] pbiqParser.QueryContext context)
            {
                var def = new QueryDefinition();
                if (context.from() != null)
                    def.From = context.from().fromElement().Select(fe =>
                    {
                        var source = new EntitySource();
                        source.Name = fe.alias().GetText();
                        source.Entity = UnescapeIdentifier(fe.entity().entity_name().GetText());

                        if (fe.expressionContainer() != null)
                            throw new NotImplementedException("to-do");

                        return source;
                    }).ToList();

                var sourceNames = def.From?.Select(f => f.Name).ToHashSet();

                if (context.where() != null)
                    def.Where = context.where().queryFilterElement().Select(qf => new QueryFilter() { Condition = ParseExprContainer(qf, sourceNames) }).ToList();

                if (context.select() != null)
                    def.Select = context.select().expression().Select(exp => ParseExprContainer(exp, sourceNames)).ToList();

                if (context.orderby() != null)
                    def.OrderBy = context.orderby().orderbySection().Select(obs => new QuerySortClause() { Expression = ParseExprContainer(obs.expression(), sourceNames), Direction = ParseDirection(obs.direction().GetText()) }).ToList();

                if (context.groupby() != null)
                    def.GroupBy = context.groupby().expression().Select(exp => ParseExprContainer(exp, sourceNames)).ToList();

                if (context.skip() != null)
                    def.Skip = int.Parse(context.skip().INTEGER().GetText());

                if (context.top() != null)
                    def.Top = int.Parse(context.top().INTEGER().GetText());

                return def;
            }

            private QueryExpressionContainer ParseExprContainer(IParseTree context, HashSet<string>? sourceNames)
            {
                var expression = new QueryExpressionVisitor(validator, false, sourceNames).VisitValidated(context);
                var container = new QueryExpressionContainer(expression);
                return container;
            }

            private QuerySortDirection ParseDirection(string v)
            {
                switch (v?.ToLower())
                {
                    case "ascending":
                        return QuerySortDirection.Ascending;
                    case "descending":
                        return QuerySortDirection.Descending;
                    default:
                        return QuerySortDirection.Unspecified;
                }
            }
        }

        class QueryExpressionVisitor : pbiqParserBaseVisitor<QueryExpression>
        {
            private readonly HashSet<string> sourceNames;
            private ParserResultValidator validator;
            private readonly bool standalone;

            public QueryExpression VisitValidated(IParseTree tree)
            {
                var res = Visit(tree);
                if (res == null)
                    throw new Exception("Failed to resolve query");

                validator.ValidateExpression(res, standalone);

                return res;
            }

            public QueryExpressionVisitor(ParserResultValidator validator, bool standalone, HashSet<string>? sourceNames = null)
            {
                this.validator = validator;
                this.standalone = standalone;
                this.sourceNames = sourceNames ?? new HashSet<string>();
            }

            public override QueryExpression VisitAndExpr([NotNull] pbiqParser.AndExprContext context)
            {
                return new QueryAndExpression() { Left = VisitValidated(context.left()), Right = VisitValidated(context.right()) };
            }

            public override QueryExpression VisitArithmenticExpr([NotNull] pbiqParser.ArithmenticExprContext context)
            {
                var left = context.expression()[0];
                var right = context.expression()[1];
                var opeartor = ParseOperator(context.BINARY_ARITHMETIC_OPERATOR().GetText());
                return new QueryArithmeticExpression()
                {
                    Left = VisitValidated(left),
                    Right = VisitValidated(right),
                    Operator = opeartor
                };
            }

            private QueryArithmeticOperatorKind ParseOperator(string v)
            {
                switch (v)
                {
                    case "-":
                        return QueryArithmeticOperatorKind.Subtract;
                    case "+":
                        return QueryArithmeticOperatorKind.Add;
                    case "*":
                        return QueryArithmeticOperatorKind.Multiply;
                    case "/":
                        return QueryArithmeticOperatorKind.Divide;
                    default:
                        throw new ArgumentException("Invalid operator");
                }
            }

            public override QueryExpression VisitScopedEvalExpr([NotNull] pbiqParser.ScopedEvalExprContext context)
            {
                var expression = VisitValidated(context.expression()[0]);
                var scopes = context.expression().Skip(1).Select(ctx => (QueryExpressionContainer)VisitValidated(ctx)).ToList();
                return new QueryScopedEvalExpression()
                {
                    Expression = expression,
                    Scope = scopes
                };
            }

            public override QueryExpression VisitHierarchyExpr([NotNull] pbiqParser.HierarchyExprContext context)
            {
                var hierarchy = UnescapeIdentifier(context.IDENTIFIER().GetText());
                var expression = VisitValidated(context.sourceRefExpr());

                return new QueryHierarchyExpression()
                {
                    Hierarchy = hierarchy,
                    Expression = expression
                };
            }

            public override QueryExpression VisitAggregationExpr([NotNull] pbiqParser.AggregationExprContext context)
            {
                var expression = VisitValidated(context.expression());
                var function = ParseFunction(context.IDENTIFIER().GetText());
                return new QueryAggregationExpression()
                {
                    Expression = expression,
                    Function = function
                };
            }

            private QueryAggregateFunction ParseFunction(string v)
            {
                return (QueryAggregateFunction)Enum.Parse(typeof(QueryAggregateFunction), v, true);
            }

            public override QueryExpression VisitSubQueryExpr([NotNull] pbiqParser.SubQueryExprContext context)
            {
                var queryDefVisitor = new QueryConstructorVisitor(validator);
                var query = queryDefVisitor.Visit(context.query());

                return new QuerySubqueryExpression() { Query = query };
            }

            public override QueryExpression VisitHierarchyLevelExpr([NotNull] pbiqParser.HierarchyLevelExprContext context)
            {
                var expression = VisitValidated(context.hierarchyExpr());
                var level = UnescapeIdentifier(context.IDENTIFIER().GetText());
                return new QueryHierarchyLevelExpression()
                {
                    Expression = expression,
                    Level = level
                };
            }

            public override QueryExpression VisitInExpr([NotNull] pbiqParser.InExprContext context)
            {
                var expression = new QueryInExpression();
                if (context.tableName() != null)
                    expression.Table = new QueryExpressionContainer(VisitValidated(context.tableName()));
                else
                {
                    if (context.inExprValues().equalityKind() != null)
                        expression.EqualityKind = context.inExprValues().equalityKind().IDENTIFIER().GetText().ToLower() == "identity" ? QueryEqualitySemanticsKind.Identity : QueryEqualitySemanticsKind.Equality;

                    expression.Values = context.inExprValues()
                        .expressionOrExpressionList()
                        .Select(el =>
                            el.expression().Select(exp => new QueryExpressionContainer(VisitValidated(exp))).ToList()
                        ).ToList();
                }
                expression.Expressions = context.nonFilterExpression().Select(exp => new QueryExpressionContainer(VisitValidated(exp))).ToList();
                return expression;
            }

            public override QueryExpression VisitSourceRefExpr([NotNull] pbiqParser.SourceRefExprContext context)
            {
                // todo: what is the schema? the schema is not saved to the minified query.
                var expression = new QuerySourceRefExpression();
                var name = UnescapeIdentifier(context.IDENTIFIER().GetText());
                if (sourceNames.Contains(name))
                    expression.Source = name;
                else
                {
                    // todo: if we do add a reference to the model (to distinguish between column and measure properties), we should check that the entity exists
                    expression.Entity = name;
                }
                return expression;
            }

            public override QueryExpression VisitIntExpr([NotNull] pbiqParser.IntExprContext context)
            {
                var text = context.INTEGER().GetText();
                if(text.Last() != 'L')
                    throw new FormatException("Invalid input");

                return new QueryIntegerConstantExpression() { Value = Int64.Parse(text.Substring(0, text.Length - 1)) };
            }

            public override QueryExpression VisitPropertyExpression_seg([NotNull] pbiqParser.PropertyExpression_segContext context)
            {
                var expression = VisitValidated((context.Parent as pbiqParser.NonFilterExpressionContext).nonFilterExpression());
                var property = UnescapeIdentifier(context.IDENTIFIER().GetText());

                // ovdje sam stao: cex.SourceRef je null, sto rezultira greskom u validaciji
                // - ne znam da li tu gresku samo treba ignorirati, usporediti generirani json nakon de-minimizacije i json seriajalizacije sa originalnim pa ako je, valjda onda treba ignorirati ovu gresku ili je tretirati kao warning
                // - nadalje, treba vidjeti kako razlikovati property od kolone i measure-a (i da li je ovo uopce bitno? da li ce sve raditi i ako je property?) ako treba, trebamo moci citati iz modela pa vidjeti jel measure ili prop (ili cu morati i minifikaciju raditi sam...ugh)
                // - zatim srediti validaciju, i errorctx i dva validatora sibam okolo, treba mi jedna klasa koja ce exposati validacijske metode i samo nju treba proslijedjivati
                var cex = new QueryExpressionContainer(expression);

                if (expression is QuerySubqueryExpression)
                {
                    
                }

                // todo: must use measure or column instead, determnine based on data model!
                return new QueryPropertyExpression()
                {
                    Expression = expression,
                    Property = property
                };
            }

            public override QueryExpression VisitLiteralExpr([NotNull] pbiqParser.LiteralExprContext context)
            {
                return new QueryLiteralExpression() { Value = context.GetText() };
            }

            public override QueryExpression VisitNotExpr([NotNull] pbiqParser.NotExprContext context)
            {
                return new QueryNotExpression() { Expression = VisitValidated(context.expression()) };
            }

            public override QueryExpression VisitBetweenExpr([NotNull] pbiqParser.BetweenExprContext context)
            {
                return new QueryBetweenExpression()
                {
                    Expression = VisitValidated(context.nonFilterExpression()),
                    LowerBound = VisitValidated(context.first()),
                    UpperBound = VisitValidated(context.second())
                };
            }

            public override QueryExpression VisitDateSpanExpr([NotNull] pbiqParser.DateSpanExprContext context)
            {
                return new QueryDateSpanExpression()
                {
                    TimeUnit = ParseTimeUnit(context.timeUnit().GetText()),
                    Expression = VisitValidated(context.expression())
                };
            }

            private TimeUnit ParseTimeUnit(string v)
            {
                return (TimeUnit)Enum.Parse(typeof(TimeUnit), v, true);
            }

            public override QueryExpression VisitComparisonExpr([NotNull] pbiqParser.ComparisonExprContext context)
            {
                var left = VisitValidated(context.first());
                var right = VisitValidated(context.second());

                return new QueryComparisonExpression()
                {
                    Left = left,
                    Right = right,
                    ComparisonKind = GetComparisonKind(context.@operator())
                };
            }

            private QueryComparisonKind GetComparisonKind(pbiqParser.OperatorContext operatorContext)
            {
                if (operatorContext.GT() != null)
                    return QueryComparisonKind.GreaterThan;
                else if (operatorContext.GTE() != null)
                    return QueryComparisonKind.GreaterThanOrEqual;
                if (operatorContext.LT() != null)
                    return QueryComparisonKind.LessThan;
                else if (operatorContext.LTE() != null)
                    return QueryComparisonKind.LessThanOrEqual;
                else
                    return QueryComparisonKind.Equal;
            }

            public override QueryExpression VisitContainsExpr([NotNull] pbiqParser.ContainsExprContext context)
            {
                return new QueryContainsExpression()
                {
                    Left = VisitValidated(context.first()),
                    Right = VisitValidated(context.second())
                };
            }

            public override QueryExpression VisitNonLeftRecursiveFilterExpression([NotNull] pbiqParser.NonLeftRecursiveFilterExpressionContext context)
            {
                var res = base.VisitNonLeftRecursiveFilterExpression(context);

                if(res == null)
                {
                    QueryExpression val = DefaultResult;
                    int childCount = context.ChildCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        if (!ShouldVisitNextChild(context, val))
                        {
                            break;
                        }

                        QueryExpression nextResult = VisitValidated(context.GetChild(i));
                        val = AggregateResult(val, nextResult);
                    }

                    return val;
                }

                return res;
            }

            protected override QueryExpression AggregateResult(QueryExpression aggregate, QueryExpression nextResult)
            {
                return nextResult ?? aggregate;
            }

            public override QueryExpression VisitOrExpr([NotNull] pbiqParser.OrExprContext context)
            {
                return new QueryOrExpression()
                {
                    Left = VisitValidated(context.left()),
                    Right = VisitValidated(context.right())
                };
            }

            public override QueryExpression VisitBoolExp([NotNull] pbiqParser.BoolExpContext context)
            {
                return new QueryBooleanConstantExpression()
                {
                    Value = context.TRUE() != null
                };
            }

            public override QueryExpression VisitNullEpr([NotNull] pbiqParser.NullEprContext context)
            {
                return new QueryNullConstantExpression();
            }
        }

        static string UnescapeIdentifier(string identifier)
        {
            identifier = identifier.Trim();
            if (identifier.StartsWith("["))
            {
                if (identifier.EndsWith("]") == false)
                    throw new FormatException("Invalid identifier");

                identifier = identifier.Substring(1, identifier.Length - 2);
                identifier = identifier.Replace("]]", "]");
            }
            return identifier;
        }

    }
}
