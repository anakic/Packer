using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.MinifiedQueryParser.QueryTransforms
{
    public class TestMinificationTransform : ModelReferenceTransformBase
    {
        protected override BaseQueryExpressionVisitor Visitor { get; } = new BaseQueryExpressionVisitor();
        QueryParser parser;
        private readonly ILogger logger;

        public TestMinificationTransform(ILogger logger)
        {
            this.parser = new QueryParser(logger);
            this.logger = logger;
        }

        protected override bool TryReadExpression(JToken expToken, out QueryExpressionContainer? expressionContainer)
        {
            // todo: compare json (expected issues: disambiguating columns and measures from properties).

            QueryExpressionContainer qec;
            if (base.TryReadExpression(expToken, out qec))
            {
                var input = qec.ToString();
                return DoTryParse(new JValue(input), s => parser.ParseExpression(s), out expressionContainer);
            }
            else
            {
                expressionContainer = default;
                return false;
            }
        }

        protected override bool TryReadFilter(JToken expToken, out FilterDefinition? filter)
        {
            FilterDefinition qec;
            if (base.TryReadFilter(expToken, out qec))
            {
                var input = qec.ToString();
                return DoTryParse(new JValue(input), s => parser.ParseFilter(s), out filter);
            }
            else
            {
                filter = default;
                return false;
            }
        }

        protected override bool TryReadQuery(JToken expToken, out QueryDefinition? query)
        {
            QueryDefinition qd;
            if (base.TryReadQuery(expToken, out qd))
            {
                var input = qd.ToString();
                return DoTryParse(new JValue(qd.ToString()), s => parser.ParseQuery(s), out query);
            }
            else
            {
                query = default;
                return false;
            }
        }

        private bool DoTryParse<T>(JToken token, Func<string, T> parseFunc, out T? res)
        {
            string input = token.ToString();
            try
            {
                if (input.StartsWith("{") || input.StartsWith("[{"))
                {
                    res = default;
                    return false;
                }
                else
                {
                    res = parseFunc(input);

                    var test = res.ToString();
                    if (test != input)
                    {
                        logger.LogError("Parse did not throw an exception but the result was incorrect.");
                        return false;
                    }

                    //logger.LogInformation("Successfully parsed: " + input);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to parse: {input}: {ex}");
                res = default;
                return false;
            }
        }
    }
}
