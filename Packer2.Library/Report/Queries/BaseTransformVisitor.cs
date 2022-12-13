using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.Report.Queries
{
    public class BaseTransformVisitor : ExtendedExpressionVisitor
    {
        public BaseTransformVisitor(string path)
        {
            Path = path;
        }

        public Dictionary<string, string> SourcesByAliasMap { get; private set; }

        public string Path { get; }

        public override void Visit(FilterDefinition filterDefinition)
        {
            SourcesByAliasMap = filterDefinition.From.ToDictionary(f => f.Name, f => f.Entity);
            base.Visit(filterDefinition);
        }

        public override void Visit(QueryDefinition queryDefinition)
        {
            SourcesByAliasMap = queryDefinition.From.ToDictionary(f => f.Name, f => f.Entity);
            base.Visit(queryDefinition);
        }
    }
}
