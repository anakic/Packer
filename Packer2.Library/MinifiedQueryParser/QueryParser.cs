using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.MinifiedQueryParser
{
    public class QueryParser
    {
        private readonly ILogger logger;

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

        public QueryParser(ILogger logger)
        {
            this.logger = logger;
        }

        public QueryDefinition ParseQuery(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.query();

            var queryDefinition = new QueryConstructorVisitor().Visit(tree);

            var errorContext = new ErrorTrackingContext(logger);
            var validator = new QueryDefinitionValidator(new QueryExpressionValidator(errorContext));
            validator.Visit(errorContext, queryDefinition);
            if (errorContext.HasError)
                throw new FormatException("Parsing query failed");

            return queryDefinition;
        }

        public FilterDefinition ParseFilter(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.query();

            var queryDefinition = new QueryConstructorVisitor().Visit(tree);


            if (queryDefinition.Select?.Count > 0 && queryDefinition.Parameters?.Count > 0 || queryDefinition.Transform?.Count > 0 || queryDefinition.GroupBy?.Count > 0 || queryDefinition.Let?.Count > 0 || queryDefinition.OrderBy?.Count > 0 || queryDefinition.Skip != null || queryDefinition.Top != null)
                throw new FormatException("Was expecting filter but got query");

            var filter = new FilterDefinition()
            {
                From = queryDefinition.From,
                Where = queryDefinition.Where
            };

            var errorContext = new ErrorTrackingContext(logger);
            var validator = new QueryDefinitionValidator(new QueryExpressionValidator(errorContext));
            validator.Visit(errorContext, filter);
            if (errorContext.HasError)
                throw new FormatException("Parsing filter failed");

            return filter;
        }

        public QueryExpression ParseExpression(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.expressionContainer();

            var expression = new QueryExpressionVisitor().Visit(tree);

            var errorContext = new ErrorTrackingContext(logger);
            var validator = new QueryExpressionValidator(errorContext);
            validator.ValidateStandaloneExpression(expression);
            if (errorContext.HasError)
                throw new FormatException("Parsing expression failed");

            return expression;
        }

        private pbiqParser CreateParser(string input)
        {
            var lexer = new pbiqLexer(CharStreams.fromString(input));
            return new pbiqParser(new CommonTokenStream(lexer));
        }

        class QueryConstructorVisitor : pbiqParserBaseVisitor<QueryDefinition>
        {
            public override QueryDefinition VisitQuery([NotNull] pbiqParser.QueryContext context)
            {
                var def = new QueryDefinition();
                if (context.from() != null)
                    def.From = context.from().fromElement().Select(fe =>
                    {
                        var source = new EntitySource();
                        source.Name = fe.alias().GetText();
                        source.Entity = fe.entity().entity_name().GetText();

                        if (fe.expressionContainer() != null)
                            throw new NotImplementedException("to-do");

                        return source;
                    }).ToList();

                var sourceNames = def.From?.Select(f => f.Name).ToHashSet();

                if (context.where() != null)
                    def.Where = context.where().queryFilter().Select(qf => new QueryFilter() { Condition = ParseExprContainer(context, sourceNames) }).ToList();

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

            private QueryExpressionContainer ParseExprContainer(ParserRuleContext context, HashSet<string>? sourceNames)
            {
                return new QueryExpressionContainer(new QueryExpressionVisitor(sourceNames).Visit(context));
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

            public QueryExpressionVisitor(HashSet<string>? sourceNames = null)
            {
                this.sourceNames = sourceNames ?? new HashSet<string>();
            }

            public override QueryExpression VisitAndExpr([NotNull] pbiqParser.AndExprContext context)
            {
                return new QueryAndExpression() { Left = Visit(context.left()), Right = Visit(context.right()) };
            }

            public override QueryExpression VisitInExpr([NotNull] pbiqParser.InExprContext context)
            {
                var expression = new QueryInExpression();
                if (context.tableName() != null)
                    expression.Table = new QueryExpressionContainer(Visit(context.tableName()));
                else
                {
                    if (context.inExprValues().equalityKind() != null)
                        expression.EqualityKind = context.inExprValues().equalityKind().IDENTIFIER().GetText().ToLower() == "identity" ? QueryEqualitySemanticsKind.Identity : QueryEqualitySemanticsKind.Equality;

                    expression.Values = context.inExprValues()
                        .expressionOrExpressionList()
                        .Select(el =>
                            el.expression().Select(exp => new QueryExpressionContainer(Visit(exp))).ToList()
                        ).ToList();
                }
                expression.Expressions = context.nonFilterExpression().Select(exp => new QueryExpressionContainer(Visit(exp))).ToList();
                return expression;
            }

            public override QueryExpression VisitSourceRefExpr([NotNull] pbiqParser.SourceRefExprContext context)
            {
                // todo: what is the schema? the schema is not saved to the minified query.
                var expression = new QuerySourceRefExpression();
                var name = context.IDENTIFIER().GetText();
                if (sourceNames.Contains(name))
                    expression.Source = name;
                else
                {
                    // todo: if we do add a reference to the model (to distinguish between column and measure properties), we should check that the entity exists
                    expression.Entity = name;
                }
                return expression;
            }

            public override QueryExpression VisitPropertyExpression([NotNull] pbiqParser.PropertyExpressionContext context)
            {
                // todo: must use measure or column instead, determnine based on data model!
                return new QueryPropertyExpression() { Expression = Visit(context.nonPropertyExpression()), Property = context.IDENTIFIER().GetText() };
            }

            public override QueryExpression VisitLiteralExpr([NotNull] pbiqParser.LiteralExprContext context)
            {
                return new QueryLiteralExpression() { Value = context.STRING_LITERAL().GetText() };
            }

            public override QueryExpression VisitNotExpr([NotNull] pbiqParser.NotExprContext context)
            {
                return new QueryNotExpression() { Expression = Visit(context.expression()) };
            }

            public override QueryExpression VisitBetweenExpr([NotNull] pbiqParser.BetweenExprContext context)
            {
                return new QueryBetweenExpression()
                {
                    Expression = Visit(context.nonFilterExpression()),
                    LowerBound = Visit(context.lbound()),
                    UpperBound = Visit(context.ubound())
                };
            }

            public override QueryExpression VisitComparisonExpr([NotNull] pbiqParser.ComparisonExprContext context)
            {
                var left = Visit(context.left());
                var right = Visit(context.right());

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
                    Left = Visit(context.left()),
                    Right = Visit(context.right())
                };
            }

            public override QueryExpression VisitOrExpr([NotNull] pbiqParser.OrExprContext context)
            {
                return new QueryOrExpression()
                {
                    Left = Visit(context.left()),
                    Right = Visit(context.right())
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
    }
}
