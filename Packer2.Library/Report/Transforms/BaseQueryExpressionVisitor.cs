using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.Report.Transforms
{
    public class BaseQueryExpressionVisitor : QueryExpressionVisitor
    {
        public Dictionary<string, string> SourcesByAliasMap { get; internal set; }
        public string InnerPath { get; internal set; }
        public string OuterPath { get; internal set; }

        protected override void Visit(QuerySourceRefExpression expression)
        {
            // todo: implement
        }

        protected override void Visit(QueryPropertyExpression expression)
        {
            // todo: implement
        }

        protected override void Visit(QueryPropertyVariationSourceExpression expression)
        {

        }

        protected override void Visit(QueryAggregationExpression expression)
        {
        }

        protected override void Visit(QueryDatePartExpression expression)
        {

        }

        protected override void Visit(QueryPercentileExpression expression)
        {

        }

        protected override void Visit(QueryFloorExpression expression)
        {

        }

        protected override void Visit(QueryDiscretizeExpression expression)
        {

        }

        protected override void Visit(QueryMemberExpression expression)
        {

        }

        protected override void Visit(QueryNativeFormatExpression expression)
        {

        }

        protected override void Visit(QueryNativeMeasureExpression expression)
        {

        }

        protected override void Visit(QueryExistsExpression expression)
        {

        }

        protected override void Visit(QueryNotExpression expression)
        {

        }

        protected override void Visit(QueryAndExpression expression)
        {

        }

        protected override void Visit(QueryOrExpression expression)
        {

        }

        protected override void Visit(QueryComparisonExpression expression)
        {

        }

        protected override void Visit(QueryContainsExpression expression)
        {

        }

        protected override void Visit(QueryStartsWithExpression expression)
        {

        }

        protected override void Visit(QueryArithmeticExpression expression)
        {

        }

        protected override void Visit(QueryEndsWithExpression expression)
        {

        }

        protected override void Visit(QueryBetweenExpression expression)
        {

        }

        protected override void Visit(QueryInExpression expression)
        {

        }

        protected override void Visit(QueryScopedEvalExpression expression)
        {

        }

        protected override void Visit(QueryFilteredEvalExpression expression)
        {

        }

        protected override void Visit(QuerySparklineDataExpression expression)
        {

        }

        protected override void Visit(QueryBooleanConstantExpression expression)
        {

        }

        protected override void Visit(QueryDateConstantExpression expression)
        {

        }

        protected override void Visit(QueryDateTimeConstantExpression expression)
        {

        }

        protected override void Visit(QueryDateTimeSecondConstantExpression expression)
        {

        }

        protected override void Visit(QueryDecadeConstantExpression expression)
        {

        }

        protected override void Visit(QueryDecimalConstantExpression expression)
        {

        }

        protected override void Visit(QueryIntegerConstantExpression expression)
        {

        }

        protected override void Visit(QueryNullConstantExpression expression)
        {

        }

        protected override void Visit(QueryStringConstantExpression expression)
        {

        }

        protected override void Visit(QueryNumberConstantExpression expression)
        {

        }

        protected override void Visit(QueryYearAndMonthConstantExpression expression)
        {

        }

        protected override void Visit(QueryYearAndWeekConstantExpression expression)
        {

        }

        protected override void Visit(QueryYearConstantExpression expression)
        {

        }

        protected override void Visit(QueryLiteralExpression expression)
        {

        }

        protected override void Visit(QueryDefaultValueExpression expression)
        {

        }

        protected override void Visit(QueryAnyValueExpression expression)
        {

        }

        protected override void Visit(QueryNowExpression expression)
        {

        }

        protected override void Visit(QueryDateAddExpression expression)
        {

        }

        protected override void Visit(QueryDateSpanExpression expression)
        {

        }

        protected override void Visit(QueryTransformOutputRoleRefExpression expression)
        {

        }

        protected override void Visit(QueryTransformTableRefExpression expression)
        {

        }

        protected override void Visit(QuerySubqueryExpression expression)
        {

        }

        protected override void Visit(QueryLetRefExpression expression)
        {

        }

        protected override void Visit(QueryRoleRefExpression expression)
        {

        }

        protected override void Visit(QuerySummaryValueRefExpression expression)
        {

        }

        protected override void Visit(QueryParameterRefExpression expression)
        {

        }

        protected override void Visit(QueryTypeOfExpression expression)
        {

        }

        protected override void Visit(QueryPrimitiveTypeExpression expression)
        {

        }

        protected override void Visit(QueryTableTypeExpression expression)
        {

        }
        protected override void Visit(QueryColumnExpression expression)
        {
        }

        protected override void Visit(QueryMeasureExpression expression)
        {
        }

        protected override void Visit(QueryHierarchyExpression expression)
        {
        }

        protected override void Visit(QueryHierarchyLevelExpression expression)
        {
        }
    }
}
