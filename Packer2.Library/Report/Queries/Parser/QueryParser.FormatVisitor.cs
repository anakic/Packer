using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.Report.QueryTransforms.Antlr
{
    public partial class QueryParser
    {
        class ClearAddedMetadataVisitor : InfoNavVisitor
        {
            protected override void Visit(QueryMeasureExpression expression)
            {
                expression.Property = $"{MEASURES_PREFIX}{expression.Property}";
                base.Visit(expression);
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                expression.Property = $"{COLUMNS_PREFIX}{expression.Property}";
                base.Visit(expression);
            }
        }
    }
}
