using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.MinifiedQueryParser;

namespace Packer2.Library.Report.Transforms
{
    public class PrettifyModelExpressionsTransform : ModelReferenceTransformBase
    {
        protected override BaseQueryExpressionVisitor Visitor { get; } = new BaseQueryExpressionVisitor();

        protected override void WriteExpression(JToken expToken, QueryExpressionContainer expObj)
        {
            expToken.Replace(expObj.ToString());
        }

        protected override void WriteFilter(JToken expToken, FilterDefinition filterObj)
        {
            expToken.Replace(filterObj.ToString());
        }

        protected override void WriteQuery(JToken expToken, QueryDefinition queryObj)
        {
            expToken.Replace(queryObj.ToString());
        }
    }

    public class RestoreModelExpressionsTransform : ModelReferenceTransformBase
    {
        protected override BaseQueryExpressionVisitor Visitor { get; } = new BaseQueryExpressionVisitor();
        QueryParser parser;

        public RestoreModelExpressionsTransform(ILogger logger)
        {
            this.parser = new QueryParser(logger);
        }

        protected override QueryExpressionContainer ReadExpression(JToken expToken)
        {
            var input = expToken.ToString();
            return parser.ParseExpression(input);
        }

        protected override FilterDefinition ReadFilter(JToken expToken)
        {
            var input = expToken.ToString();
            return parser.ParseFilter(input);
        }

        protected override QueryDefinition ReadQuery(JToken expToken)
        {
            var input = expToken.ToString();
            return parser.ParseQuery(input);
        }
    }
}
