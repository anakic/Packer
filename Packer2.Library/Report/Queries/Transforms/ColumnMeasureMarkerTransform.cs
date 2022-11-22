using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.Report.QueryTransforms.Antlr;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.MinifiedQueryParser.QueryTransforms
{
    class ColumnMeasureMarkerTransform : ReportInfoNavTransformBase
    {
        protected override QueryExpressionVisitor CreateProcessingVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap = null)
            => new MinificationMetadataMarkerVisitor(outerPath, innerPath, sourceByAliasMap);

        class MinificationMetadataMarkerVisitor : BaseQueryExpressionVisitor
        {
            public MinificationMetadataMarkerVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap)
                : base(outerPath, innerPath, sourceByAliasMap)
            {
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                expression.Property = $"{QueryParser.MEASURES_PREFIX}{expression.Property}";
                base.Visit(expression);
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                expression.Property = $"{QueryParser.COLUMNS_PREFIX}{expression.Property}";
                base.Visit(expression);
            }
        }
    }
}
