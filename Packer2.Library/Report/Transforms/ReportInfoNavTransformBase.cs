using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Queries;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Transforms
{
    /// <summary>
    /// Locates all uses of queries, filters and expressions in a report's layout file, instantiates
    /// the query/filter/expression, performs an operation on each and saves each back to its location
    /// in the layout file. Used for checking model references, renaming model references, minification
    /// and unminification of query/filter/expression code in the json file.
    /// </summary>
    public abstract class ReportInfoNavTransformBase
    {
        protected readonly ILogger logger;

        public ReportInfoNavTransformBase(ILogger? logger)
        {
            this.logger = logger ?? new DummyLogger<ReplaceModelReferenceTransform>();
        }

        public void Transform(JObject layout)
        {
            var stuffedAreaSelectors = new string[] 
            {
                "#filters",
                "sections[*].#filters",
                "sections[*].visualContainers[*].#config",
                "sections[*].visualContainers[*].#filters",
                "sections[*].#config",
                "sections[*].visualContainers[*].query",
                "sections[*].visualContainers[*].dataTransforms",
                "#config",
                "pods[*].parameters"
            };

            foreach (var selector in stuffedAreaSelectors)
            {
                var stuffedTokens = layout.SelectTokens(selector);
                foreach (var token in stuffedTokens)
                {
                    List<JObject> jObjects = new List<JObject>();
                    if (token is JArray arr)
                        jObjects.AddRange(arr.OfType<JObject>());
                    else if (token is JObject obj)
                        jObjects.Add(obj);
                    else
                        throw new InvalidOperationException();

                    var expressionSelectors = new[] { "..expr", "..expression", "..expressions[*]", "..fieldExpr", "..identityKeys[*]", "..identityValues[*]", "..scopeId" };
                    foreach (var areaJObj in jObjects)
                    {
                        foreach (var expToken in expressionSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            // these are DAX expressions, we don't want to work with those, we only want infonav expressions/filters/queries
                            if (expToken.Path.Contains(".modelExtensions["))
                                continue;

                            if (TryReadExpression(expToken, out var expression))
                            {
                                var visitor = CreateProcessingVisitor(expToken.Path);
                                PerformOperationAndLog(expression, e => visitor.VisitExpression(e), expToken.Path);
                                WriteExpression(expToken, expression!);
                            }
                        }

                        // todo: support orderby
                        // Microsoft.InfoNav.Data.Contracts.Internal.DataShapeBindingOrderBy

                        // todo: support activeProjections, e.g.
                        /*
                         "activeProjections": {
                "Rows": [
                  {
                    "Column": {
                      "Expression": {
                        "SourceRef": {
                          "Entity": "Care Changes"
                        }
                      },
                      "Property": "Resultant Simplified Service Type Crisis"
                    }
                  }
                ]
              }
                         */

                        var queryDefSelectors = new[] { "..prototypeQuery" };
                        foreach (var expToken in queryDefSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            if (TryReadQuery(expToken, out var query))
                            {
                                var visitor = CreateProcessingVisitor(expToken.Path);
                                PerformOperationAndLog(query, (q) => visitor.Visit(q), expToken.Path);
                                WriteQuery(expToken, query!);
                            }
                        }

                        var filterDefSelectors = new[] { "..filter" };
                        foreach (var expToken in filterDefSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            if (TryReadFilter(expToken, out var filter))
                            {
                                var visitor = CreateProcessingVisitor(expToken.Path);
                                PerformOperationAndLog(filter, f => visitor.Visit(f), expToken.Path);
                                WriteFilter(expToken, filter!);
                            }
                        }
                    }

                    if (token is JArray arr2)
                        token.Replace(new JArray(jObjects));
                    else if (token is JObject obj2)
                        token.Replace(jObjects.Single());
                    else
                        throw new InvalidOperationException();
                }
            }

            OnProcessingComplete(layout);
        }

        private void PerformOperationAndLog<T>(T exp, Action<T> action, string path)
        {
            var before = exp.ToString();

            action(exp);
            var after = exp.ToString();
            if (after != before)
            {
                string objectTypeFriendlyName;
                switch (exp)
                {
                    case QueryExpression:
                    case QueryExpressionContainer:
                        objectTypeFriendlyName = "Expression";
                        break;
                    case QueryDefinition:
                        objectTypeFriendlyName = "Query";
                        break;
                    case FilterDefinition:
                        objectTypeFriendlyName = "Filter";
                        break;
                    default:
                        throw new NotImplementedException();
                }

                logger.LogInformation($"{objectTypeFriendlyName} transformed: {{before}} ==> {{after}}. Location: {{path}}", before, after, path);
            }
        }

        protected virtual bool TryReadFilter(JToken expToken, out FilterDefinition? filter)
        {
            // todo: check if we need the same try-catch logic as in TryReadExpression

            if (expToken["From"] == null)
            {
                filter = null;
                return false;
            }

            filter = expToken.ToObject<FilterDefinition>()!;
            return true;
        }

        protected virtual bool TryReadQuery(JToken expToken, out QueryDefinition? queryDefinition)
        {
            // todo: check if we need the same try-catch logic as in TryReadExpression

            queryDefinition = expToken.ToObject<QueryDefinition>()!;
            return true;
        }

        protected virtual bool TryReadExpression(JToken expToken, out QueryExpressionContainer? expressionContainer)
        {
            try
            {
                expressionContainer = expToken.ToObject<QueryExpressionContainer>()!;

                // avoid using exceptions (below check) if possible
                if(expressionContainer?.Expression == null)
                    return false;

                // will throw if a nested Expression somewhere is null
                expressionContainer.ToString();

                return true;
            }
            catch
            {
                expressionContainer = default;
                return false;
            }
        }

        protected virtual void WriteFilter(JToken expToken, FilterDefinition filterObj)
        {
            // write back the updated object
            expToken.Replace(JObject.FromObject(filterObj));
        }

        protected virtual void WriteQuery(JToken expToken, QueryDefinition queryObj)
        {
            // write back the updated object
            expToken.Replace(JObject.FromObject(queryObj));
        }

        protected virtual void WriteExpression(JToken expToken, QueryExpressionContainer expObj)
        {
            // write back the updated object
            expToken.Replace(JObject.FromObject(expObj));
        }

        protected virtual void OnProcessingComplete(JObject jObject) { }

        protected abstract ExtendedExpressionVisitor CreateProcessingVisitor(string path);
    }
}
