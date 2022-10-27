using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Transforms
{
    public class Renames
    {
        class TableObjectRenames
        {
            IDictionary<string, string> renamesDict;

            public TableObjectRenames()
                : this(new Dictionary<string, string>())
            { }

            public TableObjectRenames(IDictionary<string, string> renames)
            {
                this.renamesDict = renames;
            }

            public void Add(string oldName, string newName)
                => this.renamesDict[oldName] = newName;

            public bool TryGetNewName(string originalName, out string? newName)
            {
                return renamesDict.TryGetValue(originalName, out newName);
            }
        }

        Dictionary<string, TableObjectRenames> tableRenamesDict = new Dictionary<string, TableObjectRenames>();

        public void AddRenames(string tableName, Dictionary<string, string> objectRenames)
        {
            foreach (var kvp in objectRenames)
                AddRename(tableName, kvp.Key, kvp.Value);
        }

        public void AddRename(string tableName, string oldObjectName, string newObjectName)
        {
            if (!tableRenamesDict.TryGetValue(tableName, out var renames))
                renames = tableRenamesDict[tableName] = new TableObjectRenames();
            renames.Add(oldObjectName, newObjectName);
        }

        public bool TryGetRenames(string tableName, string objectName, out string? newName)
        {
            if (
                tableRenamesDict.TryGetValue(tableName, out var tableObjectRenames_inner)
                && tableObjectRenames_inner.TryGetNewName(objectName, out newName)
            )
                return true;
            else
            {
                newName = null;
                return false;
            }
        }
    }

    public class ReplaceModelReferenceTransform : ModelReferenceTransformBase
    {
        ReplaceRefErrorsVisitor visitor;

        public ReplaceModelReferenceTransform(Renames renames, ILogger<ReplaceModelReferenceTransform>? logger = null) 
            : base(renames)
        {
            visitor = new ReplaceRefErrorsVisitor(renames, logger ?? new DummyLogger<ReplaceModelReferenceTransform>());
        }

        protected override void ProcessExpression(QueryExpressionContainer expObj, string outerPath, string innerPath)
        {
            // we only have aliases for queries and filters (they have a From clause that specifies them)
            visitor.SourcesByAliasMap = null;
            visitor.OuterPath = outerPath;
            visitor.InnerPath = innerPath;
            expObj.Expression.Accept(visitor);
        }

        protected override void ProcessFilter(FilterDefinition filterObj, string outerPath, string innerPath)
        {
            visitor.OuterPath = outerPath;
            visitor.InnerPath = innerPath;
            visitor.SourcesByAliasMap = filterObj.From.ToDictionary(f => f.Name, f => f.Entity);
            filterObj.Where.ForEach(w => w.Condition.Expression.Accept(visitor));
        }

        protected override void ProcessQuery(QueryDefinition expObj, string outerPath, string innerPath)
        {
            visitor.OuterPath = outerPath;
            visitor.InnerPath = innerPath;
            visitor.SourcesByAliasMap = expObj.From.ToDictionary(f => f.Name, f => f.Entity);

            expObj.GroupBy?.ForEach(w => w.Expression.Accept(visitor));
            expObj.OrderBy?.ForEach(w => w.Expression.Expression.Accept(visitor));
            expObj.Let?.ForEach(w => w.Expression.Accept(visitor));
            expObj.Parameters?.ForEach(w => w.Expression.Accept(visitor));
            expObj.Select?.ForEach(w => w.Expression.Accept(visitor));
            expObj.Where?.ForEach(w => w.Condition.Expression.Accept(visitor));
            expObj.Transform?.ForEach(t => 
            {
                t.Input.Parameters.ForEach(p => p.Expression.Accept(visitor));
                t.Input.Table.Columns.ForEach(c => c.Expression.Expression.Accept(visitor));
                t.Output.Table.Columns.ForEach(p => p.Expression.Expression.Accept(visitor));
            });
        }

        class ReplaceRefErrorsVisitor : BaseQueryExpressionVisitor
        {
            public int NumberOfReplacements { get; private set; } = 0;

            public string? OuterPath { get; set; }
            public string? InnerPath { get; set; }

            public Dictionary<string, string> SourcesByAliasMap { get; set; }

            private readonly Renames renames;
            private readonly ILogger logger;

            public ReplaceRefErrorsVisitor(Renames renames, ILogger traceRefErrorReporter)
            {
                this.renames = renames;
                this.logger = traceRefErrorReporter;
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                Process(expression);
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                Process(expression);
            }

            private void Process(QueryPropertyExpression expression)
            {
                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source];
                
                if (renames.TryGetRenames(sourceName, expression.Property, out var newName))
                {
                    var originalName = expression.Property;
                    expression.Property = newName;
                    logger.LogInformation("Replaced a reference to an object in table '{tableName}' from '{oldName}' to '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", sourceName, originalName, newName, OuterPath, InnerPath);
                    NumberOfReplacements++;
                }
            }
        }
    }

    public abstract class ModelReferenceTransformBase : IReportTransform
    {
        private readonly Renames renames;

        public ModelReferenceTransformBase(Renames renames)
        {
            this.renames = renames;
        }

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

            return model;
        }

        protected abstract void ProcessFilter(FilterDefinition filterObj, string outerPath, string innerPath);

        protected abstract void ProcessQuery(QueryDefinition expObj, string outerPath, string innerPath);

        protected abstract void ProcessExpression(QueryExpressionContainer expObj, string outerPath, string innerPath);
    }
}
