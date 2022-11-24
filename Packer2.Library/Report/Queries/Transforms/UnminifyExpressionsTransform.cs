using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.QueryTransforms.Antlr;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.MinifiedQueryParser.QueryTransforms
{
    class UnminifyExpressionsLayoutJsonTransform : ReportInfoNavTransformBase
    {
        protected override QueryExpressionVisitor CreateProcessingVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap = null)
            => new BaseQueryExpressionVisitor(outerPath, innerPath, sourceByAliasMap);

        QueryParser parser;
        private readonly ILogger logger;

        // todo: pass in glossary

        public UnminifyExpressionsLayoutJsonTransform(ColumnsAndMeasuresGlossary glossary, ILogger logger)
        {
            parser = new QueryParser(glossary, logger);
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

        protected override bool TryReadQuery(JToken expToken, out QueryDefinition? query)
        {
            var parent = (JProperty)expToken.Parent!;
            var selectListProp = parent.Parent.SelectToken($"#{parent.Name}SelectList");
            if (selectListProp == null)
            {
                query = default;
                return false;
            }

            var selectList = selectListProp.ToObject<string[]>();
            selectListProp.Parent.Remove();

            var result = DoTryParse(expToken, parser.ParseQuery, out query);
            if (result)
            {
                query.Select.Zip(selectList).ToList().ForEach(ec => ec.First.Name = ec.Second);
            }

            return result;
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
