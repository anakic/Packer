using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.MinifiedQueryParser;
using System.Diagnostics;

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
        private readonly ILogger logger;

        public RestoreModelExpressionsTransform(ILogger logger)
        {
            this.parser = new QueryParser(logger);
            this.logger = logger;
        }

        protected override bool TryReadExpression(JToken expToken, out QueryExpressionContainer? expressionContainer)
        {
            return DoTryParse(expToken, s => parser.ParseExpression(s), out expressionContainer);
        }

        protected override bool TryReadFilter(JToken expToken, out FilterDefinition? filter)
        {
            return DoTryParse(expToken, parser.ParseFilter, out filter);
        }

        protected override bool TryReadQuery(JToken expToken, out QueryDefinition? filter)
        {
            return DoTryParse(expToken, parser.ParseQuery, out filter);
        }

        private bool DoTryParse<T>(JToken token, Func<string, T> parseFunc, out T? res)
        {
            string input = token.ToString();
            try
            {
                if (input.StartsWith("{") || input.StartsWith("["))
                {
                    res = default;
                    return false;
                }
                else
                {
                    res = parseFunc(input);

                    var test = res.ToString();
                    if (test != input)
                        throw new FormatException("Parse did not throw an exception but the result was incorrect.");

                    logger.LogInformation("Succesfully parsed: " + input);

                    return true;
                }
            }
            catch(Exception ex)
            {
                Trace.WriteLine($">>> Failed to parse: {input}: {ex}");
                logger.LogError($"Failed to parse: {input}: {ex}");
                res = default;
                return false;
            }
        }
    }
}
