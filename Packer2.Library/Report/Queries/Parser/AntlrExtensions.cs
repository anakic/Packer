using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Linq.Expressions;

namespace Packer2.Library.Report.QueryTransforms.Antlr
{
    public static class AntlrExtensions
    {
        public static T Parent<T>(this ITree node) where T : RuleContext
            => (T)node.Parent;
    }
}
