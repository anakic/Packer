using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

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

                protected override void Visit(QueryScopedEvalExpression expression)
                {
                    // base class firing false positive here as well (column & hierarchy is null)
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
            var tree = parser.root();

            var queryDefinition = new QueryConstructorVisitor(validator).Visit(tree);
            validator.ValidateQuery(queryDefinition);
            
            return queryDefinition;
        }

        public FilterDefinition ParseFilter(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.root();
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

            private string ReadStringLiteral(ITerminalNode node)
            {
                if (node == null)
                    return null;

                var text = node.GetText();
                return text.Substring(1, text.Length - 2);
            }

            public QueryTransform ParseTransform(pbiqParser.TransformContext context, HashSet<string> sourceNames)
            {
                QueryTransformInput input = new QueryTransformInput();
                QueryTransformOutput output = new QueryTransformOutput();

                if (context.parameters() != null)
                    input.Parameters = context.parameters().parameter().Select(pc => ParseExprContainer(pc, sourceNames, UnescapeIdentifier(pc.alias().GetText()))).ToList();

                if (context.inputTable() != null)
                {
                    input.Table = new QueryTransformTable()
                    {
                        Name = context.inputTable().alias().GetText(),
                        Columns = context.inputTable().tableColumn().Select(tc => new QueryTransformTableColumn() { Role = ReadStringLiteral(tc.STRING_LITERAL()), Expression = ParseExprContainer(tc.expression(), sourceNames, UnescapeIdentifier(tc.alias().GetText())) }).ToList()
                    };
                    sourceNames.Add(input.Table.Name);
                }

                if (context.outputTable() != null)
                {
                    output.Table = new QueryTransformTable()
                    {
                        Name = context.outputTable().alias().GetText(),
                        Columns = context.outputTable().tableColumn().Select(tc => new QueryTransformTableColumn() { Role = ReadStringLiteral(tc.STRING_LITERAL()), Expression = ParseExprContainer(tc.expression(), sourceNames, UnescapeIdentifier(tc.alias().GetText())) }).ToList()
                    };
                    sourceNames.Add(output.Table.Name);
                }

                return new QueryTransform()
                {
                    Name = UnescapeIdentifier(context.identifier().GetText()),
                    Input = input,
                    Output = output,
                    Algorithm = ReadStringLiteral(context.algorithm().STRING_LITERAL())
                };
            }

            public QueryConstructorVisitor(ParserResultValidator validator)
            {
                this.validator = validator;
            }

            public override QueryDefinition VisitRoot([NotNull] pbiqParser.RootContext context)
            {
                return VisitQuery(context.query());
            }

            public override QueryDefinition VisitQuery([NotNull] pbiqParser.QueryContext context)
            {
                var def = new QueryDefinition();
                if (context.from() != null)
                    def.From = context.from().fromElement().Select(fe =>
                    {
                        var source = new EntitySource();
                        source.Name = fe.alias().GetText();

                        if(fe.entity_name() != null)
                            source.Entity = UnescapeIdentifier(fe.entity_name().GetText());

                        else
                        {
                            var subQuery = ParseExprContainer(fe.subQueryExpr());
                            source.Expression = subQuery;
                            source.Type = EntitySourceType.Expression;
                        }

                        return source;
                    }).ToList();

                var sourceNames = def.From?.Select(f => f.Name).ToHashSet();

                if (context.where() != null)
                    def.Where = context.where().queryFilterElement().Select(qf => new QueryFilter() { Condition = ParseExprContainer(qf, sourceNames) }).ToList();

                if (context.transform()?.Count() > 0)
                {
                    def.Transform = context.transform().Select(t => ParseTransform(t, sourceNames)).ToList();
                }

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

                var asJson = Newtonsoft.Json.JsonConvert.SerializeObject(def, Newtonsoft.Json.Formatting.Indented);

                return def;
            }

            private QueryExpressionContainer ParseExprContainer(IParseTree context, HashSet<string>? sourceNames = null, string alias = null)
            {
                var expression = new QueryExpressionVisitor(validator, false, sourceNames).VisitValidated(context);
                var container = new QueryExpressionContainer(expression);
                if (alias != null)
                    container.Name = alias;
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

            public override QueryExpression VisitRoleRefExpression([NotNull] pbiqParser.RoleRefExpressionContext context)
            {
                return new QueryRoleRefExpression() { Role = context.QUOTED_IDENTIFIER().GetText().TrimStart('[').TrimEnd(']') };
            }

            public override QueryExpression VisitArithmenticExpr([NotNull] pbiqParser.ArithmenticExprContext context)
            {
                var left = context.left();
                var right = context.right();
                var opeartor = ParseOperator(context.binary_arithmetic_operator().GetText());
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

            public override QueryExpression VisitVariationExpr([NotNull] pbiqParser.VariationExprContext context)
            {
                var expression = VisitValidated(context.sourceRefExpr());
                if (context.identifier().Count() != 2)
                    throw new FormatException("Variation syntax is: expr.variation(property, name). Expecting two identifiers (property and name)!");
                var property = UnescapeIdentifier(context.identifier().ElementAt(0).GetText());
                var name = UnescapeIdentifier(context.identifier().ElementAt(1).GetText());

                return new QueryPropertyVariationSourceExpression() 
                {
                    Expression = expression,
                    Property = property,
                    Name = name
                };
            }

            public override QueryExpression VisitHierarchyExpr([NotNull] pbiqParser.HierarchyExprContext context)
            {
                var hierarchy = UnescapeIdentifier(context.identifier().GetText());
                var expression = VisitValidated(context.hierarchySource());

                return new QueryHierarchyExpression()
                {
                    Hierarchy = hierarchy,
                    Expression = expression
                };
            }

            public override QueryExpression VisitAggregationExpr([NotNull] pbiqParser.AggregationExprContext context)
            {
                var expression = VisitValidated(context.expression());
                var function = ParseFunction(context.identifier().GetText());
                return new QueryAggregationExpression()
                {
                    Expression = expression,
                    Function = function
                };
            }

            private string ReadStringLiteral(ITerminalNode node)
            {
                var text = node.GetText();
                return text.Substring(1, text.Length - 2);
            }

            public override QueryExpression VisitTransformOutputRoleRefExpr([NotNull] pbiqParser.TransformOutputRoleRefExprContext context)
            {
                return new QueryTransformOutputRoleRefExpression() { Role = ReadStringLiteral(context.STRING_LITERAL()) };
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
                var level = UnescapeIdentifier(context.identifier().GetText());
                return new QueryHierarchyLevelExpression()
                {
                    Expression = expression,
                    Level = level
                };
            }

            public override QueryExpression VisitInExpr([NotNull] pbiqParser.InExprContext context)
            {
                var expression = new QueryInExpression();
                if (context.sourceRefExpr() != null)
                    expression.Table = new QueryExpressionContainer(VisitValidated(context.sourceRefExpr()));
                else
                {
                    if (context.inExprValues().inExprEqualityKind() != null)
                        expression.EqualityKind = context.inExprValues().inExprEqualityKind().identifier().GetText().ToLower() == "identity" ? QueryEqualitySemanticsKind.Identity : QueryEqualitySemanticsKind.Equality;

                    expression.Values = context.inExprValues()
                        .expressionOrExpressionList()
                        .Select(el => el.expression().Select(exp => new QueryExpressionContainer(VisitValidated(exp))).ToList()).ToList();
                }

                if(context.primary_expression() != null)
                    expression.Expressions = new List<QueryExpressionContainer>() { VisitValidated(context.primary_expression()) };
                else
                    expression.Expressions = context.expression().Select(exp => new QueryExpressionContainer(VisitValidated(exp))).ToList();
                return expression;
            }

            public override QueryExpression VisitSourceRefExpr([NotNull] pbiqParser.SourceRefExprContext context)
            {
                // todo: what is the schema? the schema is not saved to the minified query.
                var expression = new QuerySourceRefExpression();
                var name = UnescapeIdentifier(context.identifier().GetText());
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

            public override QueryExpression VisitPropertyExpression([NotNull] pbiqParser.PropertyExpressionContext context)
            {
                var expression = VisitValidated((ParserRuleContext)context.sourceRefExpr() ?? context.subQueryExpr());
                var property = UnescapeIdentifier(context.identifier().GetText());

                // todo: must use measure or column instead, determnine based on data model!
                return new QueryPropertyExpression()
                {
                    Expression = expression,
                    Property = property
                };
            }


            public override QueryExpression VisitEncodedLiteralExpr([NotNull] pbiqParser.EncodedLiteralExprContext context)
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
                    Expression = VisitValidated(context.primary_expression()),
                    LowerBound = VisitValidated(context.left()),
                    UpperBound = VisitValidated(context.right())
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

            public override QueryExpression VisitCompareExpr([NotNull] pbiqParser.CompareExprContext context)
            {
                var left = VisitValidated(context.primary_expression());
                var right = VisitValidated(context.right());

                return new QueryComparisonExpression()
                {
                    Left = left,
                    Right = right,
                    ComparisonKind = GetComparisonKind(context.comparisonOperator())
                };
            }

            private QueryComparisonKind GetComparisonKind(pbiqParser.ComparisonOperatorContext operatorContext)
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
                    Left = VisitValidated(context.primary_expression()),
                    Right = VisitValidated(context.right())
                };
            }

            protected override QueryExpression AggregateResult(QueryExpression aggregate, QueryExpression nextResult)
            {
                return nextResult ?? aggregate;
            }

            public override QueryExpression VisitLogicalExpr([NotNull] pbiqParser.LogicalExprContext context)
            {
                var left = VisitValidated(context.left());
                var right = VisitValidated(context.right());
                
                if (context.binary_logic_operator().GetText().ToLower() == "and")
                    return new QueryAndExpression() { Left = left, Right = right };
                else if (context.binary_logic_operator().GetText().ToLower() == "or")
                    return new QueryOrExpression() { Left = left, Right = right };
                else
                    throw new FormatException("Invalid logical operator");
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
