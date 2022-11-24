using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.Report.Transforms
{
    public class BaseQueryExpressionVisitor : DefaultQueryExpressionVisitor
    {
        public BaseQueryExpressionVisitor(string outerPath, string innerPath, Dictionary<string, string> sourcesByAliasMap)
        {
            SourcesByAliasMap = sourcesByAliasMap;
            InnerPath = innerPath;
            OuterPath = outerPath;
        }

        public Dictionary<string, string> SourcesByAliasMap { get; }
        public string InnerPath { get; }
        public string OuterPath { get; }
    }
}
