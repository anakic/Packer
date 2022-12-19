using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.Report.QueryTransforms.Antlr
{
    public partial class QueryParser
    {
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

                protected override void Visit(QueryMeasureExpression expression)
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

                protected override void Visit(QueryColumnExpression expression)
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

                protected override void Visit(QueryHierarchyExpression expression)
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
    }
}
