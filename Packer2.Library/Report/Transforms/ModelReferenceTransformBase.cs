﻿using DataModelLoader.Report;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Transforms
{
    public abstract class ModelReferenceTransformBase : IReportTransform
    {
        record Finding(string parentPath, string nestedPath, string text, object deserialized, Exception ex);
        public PowerBIReport Transform(PowerBIReport model)
        {
            var stuffedAreaSelectors = new string[] 
            {
                "filters",
                "sections[*].filters",
                "sections[*].visualContainers[*].config",
                "sections[*].visualContainers[*].filters",
                "sections[*].config",
                "sections[*].visualContainers[*].query",
                "sections[*].visualContainers[*].dataTransforms",
                "config",
                "pods[*].parameters" 
            };

            foreach (var selector in stuffedAreaSelectors)
            {
                var stuffedTokens = model.Layout.SelectTokens(selector);
                foreach (var token in stuffedTokens)
                {
                    List<JObject> jObjects = new List<JObject>();
                    var tok = JsonConvert.DeserializeObject(token.ToString());
                    if (tok is JArray arr)
                        jObjects.AddRange(arr.OfType<JObject>());
                    else if (tok is JObject obj)
                        jObjects.Add(obj);
                    else
                        throw new InvalidOperationException();

                    foreach (var areaJObj in jObjects)
                    {
                        var expressionSelectors = new[] { "..expr", "..expression", "..expressions[*]", "..fieldExpr", "..identityKeys[*]", "..identityValues[*]", "..scopeId" };
                        foreach (var expToken in expressionSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            // one of the above selectors returns some objects that aren't really an expressionContainer, skip those
                            var expObj = expToken.ToObject<QueryExpressionContainer>()!;
                            // todo: use has property "Expression" as selector filter
                            if (expObj.Expression == null)
                                continue;

                            ProcessExpression(expObj, token.Path, expToken.Path);

                            // write back the updated object
                            expToken.Replace(JObject.FromObject(expObj));
                        }

                        var queryDefSelectors = new[] { "..prototypeQuery" };
                        foreach (var expToken in queryDefSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            var queryObj = expToken.ToObject<QueryDefinition>()!;
                            ProcessQuery(queryObj, token.Path, expToken.Path);
                            // write back the updated object
                            expToken.Replace(JObject.FromObject(queryObj));
                        }

                        var filterDefSelectors = new[] { "..filter" };
                        foreach (var expToken in filterDefSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            // this is the outer filter node, we want the inner one
                            // todo: use has property "From" as selector filter
                            if (expToken["filter"] != null)
                                continue;

                            var filterObj = expToken.ToObject<FilterDefinition>()!;
                            ProcessFilter(filterObj, token.Path, expToken.Path);

                            // write back the updated object
                            expToken.Replace(JObject.FromObject(filterObj));
                        }
                    }

                    // write back the stuffed string
                    if (tok is JArray arr2)
                        token.Replace(new JArray(jObjects).ToString(Formatting.None));
                    else if (tok is JObject obj2)
                        token.Replace(jObjects.Single().ToString(Formatting.None));
                    else
                        throw new InvalidOperationException();
                }
            }

            OnProcessingComplete(model);

            return model;
        }

        protected virtual void OnProcessingComplete(PowerBIReport model) { }

        protected abstract void ProcessFilter(FilterDefinition filterObj, string outerPath, string innerPath);

        protected abstract void ProcessQuery(QueryDefinition expObj, string outerPath, string innerPath);

        protected abstract void ProcessExpression(QueryExpressionContainer expObj, string outerPath, string innerPath);
    }
}
