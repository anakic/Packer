using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.Report.QueryTransforms.Antlr
{
    public partial class QueryParser
    {
        class AddMissingMetadataVisitor : InfoNavVisitor
        {
            protected override void Visit(QueryMeasureExpression expr)
            {
                if (expr.Property.StartsWith(MEASURES_PREFIX))
                    expr.Property = expr.Property.Substring(MEASURES_PREFIX.Length);
                else
                    throw new FormatException("Measure name did not start with expected prefix!");
            }

            protected override void Visit(QueryColumnExpression expr)
            {
                if (expr.Property.StartsWith(COLUMNS_PREFIX))
                    expr.Property = expr.Property.Substring(COLUMNS_PREFIX.Length);
                else
                    throw new FormatException("Column name did not start with expected prefix!");
            }
        }
    }
}
