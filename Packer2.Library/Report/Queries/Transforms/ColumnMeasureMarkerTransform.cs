using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.Report.QueryTransforms.Antlr;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.MinifiedQueryParser.QueryTransforms
{
    public class ColumnMeasureMarkerTransform : ModelReferenceTransformBase
    {
        protected override BaseQueryExpressionVisitor Visitor => new MinificationMetadataMarkerVisitor();

        class MinificationMetadataMarkerVisitor : BaseQueryExpressionVisitor
        {
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
