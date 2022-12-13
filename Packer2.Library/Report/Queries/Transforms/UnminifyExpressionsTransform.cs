using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Queries;
using Packer2.Library.Report.QueryTransforms.Antlr;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.MinifiedQueryParser.QueryTransforms
{
    class UnminifyExpressionsLayoutJsonTransform : ReportInfoNavTransformBase
    {
        protected override ExtendedExpressionVisitor CreateProcessingVisitor(string path)
            => new BaseTransformVisitor(path);

        QueryParser parser;
        private readonly ILogger logger;

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
            var selectListProp = parent.Parent!.SelectToken($"#{parent.Name}SelectList");
            if (selectListProp == null)
            {
                query = default;
                return false;
            }

            var selectList = selectListProp.ToObject<string[]>()!;
            selectListProp.Parent!.Remove();

            var result = DoTryParse(expToken, parser.ParseQuery, out query);
            if (result)
            {
                query!.Select.Zip(selectList).ToList().ForEach(ec => ec.First.Name = ec.Second);
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
                    logger.LogTrace("Token at path '{path}' is not a serialized query/filter/expression. Skipping.", token.Path);
                    res = default;
                    return false;
                }
                else
                {
                    logger.LogTrace("Parsing token at path '{path}'", token.Path);

                    res = parseFunc(input);

                    var test = res.ToString();
                    if (test != input)
                    {
                        logger.LogError("Parse did not throw an exception but the result was incorrect. Original query was '{input}', but the parsed result is {result}", input, test);
                        return false;
                    }

                    logger.LogTrace("Successfully parsed: {input}", input);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError("An exception occured while parsing: {input}: {ex}", input, ex);
                res = default;
                return false;
            }
        }
    }
}
