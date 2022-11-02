using DataModelLoader.Report;
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

                            WriteExpression(expToken, expObj);
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
                            var queryObj = expToken.ToObject<QueryDefinition>()!;
                            ProcessQuery(queryObj, token.Path, expToken.Path);
                            WriteQuery(expToken, queryObj);
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

                            WriteFilter(expToken, filterObj);
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

        protected virtual void OnProcessingComplete(PowerBIReport model) { }

        // todo: should not force deriving from BaseQueryExpressionVisitor, we could make BaseQueryExpressionVisitor
        // a decorator around the user's visitor object. The properties of BaseQueryExpressionVisitor are only used
        // by this class internally, they are not by client code.
        protected abstract BaseQueryExpressionVisitor Visitor { get; }

        protected void ProcessExpression(QueryExpressionContainer expObj, string outerPath, string innerPath)
        {
            // we only have aliases for queries and filters (they have a From clause that specifies them)
            Visitor.SourcesByAliasMap = null;
            Visitor.OuterPath = outerPath;
            Visitor.InnerPath = innerPath;
            expObj.Expression.Accept(Visitor);
        }

        protected void ProcessFilter(FilterDefinition filterObj, string outerPath, string innerPath)
        {
            Visitor.OuterPath = outerPath;
            Visitor.InnerPath = innerPath;
            Visitor.SourcesByAliasMap = filterObj.From.ToDictionary(f => f.Name, f => f.Entity);
            filterObj.Where.ForEach(w => w.Condition.Expression.Accept(Visitor));
        }

        protected virtual void ProcessQuery(QueryDefinition expObj, string outerPath, string innerPath)
        {
            Visitor.OuterPath = outerPath;
            Visitor.InnerPath = innerPath;
            Visitor.SourcesByAliasMap = expObj.From.ToDictionary(f => f.Name, f => f.Entity);

            expObj.GroupBy?.ForEach(w => w.Expression.Accept(Visitor));
            expObj.OrderBy?.ForEach(w => w.Expression.Expression.Accept(Visitor));
            expObj.Let?.ForEach(w => w.Expression.Accept(Visitor));
            expObj.Parameters?.ForEach(w => w.Expression.Accept(Visitor));
            expObj.Select?.ForEach(w => w.Expression.Accept(Visitor));
            expObj.Where?.ForEach(w => w.Condition.Expression.Accept(Visitor));
            expObj.Transform?.ForEach(t =>
            {
                t.Input.Parameters.ForEach(p => p.Expression.Accept(Visitor));
                t.Input.Table.Columns.ForEach(c => c.Expression.Expression.Accept(Visitor));
                t.Output.Table.Columns.ForEach(p => p.Expression.Expression.Accept(Visitor));
            });
        }
    }
}
