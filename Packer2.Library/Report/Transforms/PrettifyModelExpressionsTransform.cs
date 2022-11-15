using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.MinifiedQueryParser;
using System.Diagnostics;

namespace Packer2.Library.Report.Transforms
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
                var success = DoTryParse(new JValue(qec.ToString()), s => parser.ParseExpression(s), out expressionContainer);
                if (!success)
                    logger.LogError($"Failed to parse minified query: {input}");
                else if (input != expressionContainer.ToString())
                    logger.LogError("Unminified filter does not correspond to minified version");
                else
                    logger.LogInformation("Succesfully minified and unminified expression: " + input);
                return success;
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
                var success = DoTryParse(new JValue(input), s => parser.ParseFilter(s), out filter);
                if (!success)
                    logger.LogError($"Failed to parse minified filter: {input}");
                else if (input != filter.ToString())
                    logger.LogError("Unminified filter does not correspond to minified version");
                else
                    logger.LogInformation("Succesfully minified and unminified query: " + input);
                return success;
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

                var success = DoTryParse(new JValue(qd.ToString()), s => parser.ParseQuery(s), out query);

                if (!success)
                    throw new Exception($"Failed to parse minified query: {input}");
                else if (input != query.ToString())
                    throw new Exception("Unminified query does not correspond to minified version");
                else
                    logger.LogInformation("Succesfully minified and unminified query: " + input);
                return success;
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
                        throw new FormatException("Parse did not throw an exception but the result was incorrect.");

                    // logger.LogInformation("Succesfully parsed: " + input);

                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($">>> Failed to parse: {input}: {ex}");
                logger.LogError($"Failed to parse: {input}: {ex}");
                res = default;
                return false;
            }
        }
    }

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

                    // logger.LogInformation("Succesfully parsed: " + input);

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
