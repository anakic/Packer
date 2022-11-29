using DataModelLoader.Report;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                                ProcessExpression(expression!, token.Path, expToken.Path);
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
                                ProcessQuery(query!, token.Path, expToken.Path);
                                WriteQuery(expToken, query!);
                            }
                        }

                        var filterDefSelectors = new[] { "..filter" };
                        foreach (var expToken in filterDefSelectors.SelectMany(areaJObj.SelectTokens).ToArray())
                        {
                            if (TryReadFilter(expToken, out var filter))
                            {
                                ProcessFilter(filter!, token.Path, expToken.Path);
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

        protected virtual bool TryReadFilter(JToken expToken, out FilterDefinition? filter)
        {
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
            queryDefinition = expToken.ToObject<QueryDefinition>()!;
            return true;
        }

        protected virtual bool TryReadExpression(JToken expToken, out QueryExpressionContainer? expressionContainer)
        {
            try
            {
                expressionContainer = expToken.ToObject<QueryExpressionContainer>()!;
                return (expressionContainer?.Expression != null);
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

        protected abstract QueryExpressionVisitor CreateProcessingVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap = null);

        protected void ProcessExpression(QueryExpressionContainer expression, string outerPath, string innerPath)
        {
            // we only have aliases for queries and filters (they have a From clause that specifies them)
            var visitor = CreateProcessingVisitor(outerPath, innerPath);
            expression.Expression.Accept(visitor);
        }

        protected void ProcessFilter(FilterDefinition filterObj, string outerPath, string innerPath)
        {
            var visitor = CreateProcessingVisitor(outerPath, innerPath, filterObj.From.ToDictionary(f => f.Name, f => f.Entity));
            filterObj.Where.ForEach(w => w.Condition.Expression.Accept(visitor));
        }

        protected virtual void ProcessQuery(QueryDefinition query, string outerPath, string innerPath)
        {
            var visitor = CreateProcessingVisitor(outerPath, innerPath, query.From.ToDictionary(f => f.Name, f => f.Entity));
            query.From.ForEach(es => es.Expression?.Expression.Accept(visitor));
            query.GroupBy?.ForEach(w => w.Expression.Accept(visitor));
            query.OrderBy?.ForEach(w => w.Expression.Expression.Accept(visitor));
            query.Let?.ForEach(w => w.Expression.Accept(visitor));
            query.Parameters?.ForEach(w => w.Expression.Accept(visitor));
            query.Select?.ForEach(w => w.Expression.Accept(visitor));
            query.Where?.ForEach(w => w.Condition.Expression.Accept(visitor));
            query.Transform?.ForEach(t =>
            {
                t.Input.Parameters.ForEach(p => p.Expression.Accept(visitor));
                t.Input.Table.Columns.ForEach(c => c.Expression.Expression.Accept(visitor));
                t.Output.Table.Columns.ForEach(p => p.Expression.Expression.Accept(visitor));
            });
        }
    }
}
