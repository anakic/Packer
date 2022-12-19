using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.Report.Queries
{
    public class BaseTransformVisitor : ExtendedExpressionVisitor
    {
        public Dictionary<string, EntitySource> SourcesByAliasMap { get; private set; }

        public override void Visit(FilterDefinition filterDefinition)
        {
            SourcesByAliasMap = filterDefinition.From.ToDictionary(f => f.Name, f => f);
            base.Visit(filterDefinition);
        }

        public override void Visit(QueryDefinition queryDefinition)
        {
            SourcesByAliasMap = queryDefinition.From.ToDictionary(f => f.Name, f => f);
            base.Visit(queryDefinition);
        }
    }
}
