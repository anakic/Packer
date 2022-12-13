using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Queries;
using Packer2.Library.Tools;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using static Microsoft.InfoNav.Common.DijkstraAlgorithm;

namespace Packer2.Library.Report.Transforms
{
    public record TableObjectMapping(string OldObjectName, string NewObjectName, string NewTableName);

    public class TableMappings
    {
        Dictionary<string, TableObjectMapping> objectMoves = new Dictionary<string, TableObjectMapping>();
        public IEnumerable<TableObjectMapping> ObjectMoves => objectMoves.Values;

        public TableMappings(string tableName)
        {
            TableName = tableName;
            NewTableName = tableName;
        }

        public TableMappings MapTo(string newName)
        {
            NewTableName = newName;
            return this;
        }

        public TableMappings MapObjectTo(string name, string newName, string? newParentTable = null)
        {
            objectMoves.Add(name, new TableObjectMapping(name, newName, newParentTable ?? NewTableName));
            return this;
        }

        public bool TryGetMapping(string originalName, [MaybeNullWhen(false)] out TableObjectMapping change)
            => objectMoves.TryGetValue(originalName, out change);

        public string TableName { get; }
        public string NewTableName { get; private set; }
    }

    public class Mappings
    {
        Dictionary<string, TableMappings> tableMappings = new Dictionary<string, TableMappings>();
        public TableMappings Table(string name)
        {
            if (tableMappings.TryGetValue(name, out var trc))
                return trc;
            else
                return tableMappings[name] = new TableMappings(name);
        }

        public bool TryGetNewTableName(string originalName, [MaybeNullWhen(false)] out string newTableName)
        {
            if (tableMappings.TryGetValue(originalName, out var tableObjMappings))
            {
                if (tableObjMappings.NewTableName != tableObjMappings.TableName)
                {
                    newTableName = tableObjMappings.NewTableName;
                    return true;
                }
            }

            newTableName = null;
            return false;
        }

        public bool TryGetMappedObjectInfo(string originalTableName, string originalObjectName, [MaybeNullWhen(false)] out string newTableName, [MaybeNullWhen(false)] out string newObjectName)
        {
            if (tableMappings.TryGetValue(originalTableName, out var tableObjMappings))
            {
                if (tableObjMappings.TryGetMapping(originalObjectName, out var change))
                {
                    newTableName = change.NewTableName;
                    newObjectName = change.NewObjectName;
                    return true;
                }
            }

            newTableName = null;
            newObjectName = null;
            return false;
        }
    }

    public class ReplaceModelReferenceTransform : ReportInfoNavTransformBase
    {
        private readonly Mappings renames;
        private readonly ILogger<ReplaceModelReferenceTransform> logger;

        int numberOfReplacements;

        public ReplaceModelReferenceTransform(Mappings renames, ILogger<ReplaceModelReferenceTransform>? logger = null)
        {
            this.renames = renames;
            this.logger = logger ?? new DummyLogger<ReplaceModelReferenceTransform>();
        }

        protected override void OnProcessingComplete(JObject jObj)
        {
            if (numberOfReplacements == 0)
                logger.LogInformation("No references were found, nothing to replace");
            else
                logger.LogInformation($"A total of {numberOfReplacements} replacements were made.");
        }

        protected override ExtendedExpressionVisitor CreateProcessingVisitor(string path)
            => new ReplaceRefErrorsVisitor(path, renames, logger, () => numberOfReplacements++);

        class ReplaceRefErrorsVisitor : BaseTransformVisitor
        {
            private readonly Mappings renames;
            private readonly ILogger logger;
            private readonly Action incrementReplaceCountAction;

            public ReplaceRefErrorsVisitor(string path, Mappings renames, ILogger traceRefErrorReporter, Action incrementReplaceCountAction)
                : base(path)
            {
                this.renames = renames;
                this.logger = traceRefErrorReporter;
                this.incrementReplaceCountAction = incrementReplaceCountAction;
            }

            public override void Visit(FilterDefinition filterDefinition)
            {
                base.Visit(filterDefinition);

                var takenAliases = new HashSet<string>();
                var sources = new List<EntitySource>();
                foreach (var kvp in aliasedPropertyExpressions)
                {
                    var alias = FindNextAvailableAlias(kvp.Key.Substring(0, 1).ToLower(), takenAliases);
                    takenAliases.Add(alias);
                    foreach (var exp in kvp.Value)
                    {
                        switch (exp)
                        {
                            case QueryPropertyExpression qpe:
                                qpe.Expression.SourceRef.Source = alias;
                                break;
                            case QueryHierarchyExpression qhe:
                                qhe.Expression.SourceRef.Source = alias;
                                break;
                            default:
                                throw new NotImplementedException("Currently only support moving/renaming measures, columns and hierarchies");
                        }
                    }
                    sources.Add(new EntitySource() { Entity = kvp.Key, Name = alias });
                }
                SyncSources(sources, filterDefinition.From);
            }

            public override void Visit(QueryDefinition queryDefinition)
            {
                base.Visit(queryDefinition);

                var takenAliases = new HashSet<string>();
                var sources = new List<EntitySource>();
                foreach (var kvp in aliasedPropertyExpressions)
                {
                    var alias = FindNextAvailableAlias(kvp.Key.Substring(0, 1).ToLower(), takenAliases);
                    takenAliases.Add(alias);
                    foreach (var exp in kvp.Value)
                    {
                        switch (exp)
                        {
                            case QueryPropertyExpression qpe:
                                qpe.Expression.SourceRef.Source = alias;
                                break;
                            case QueryHierarchyExpression qpe:
                                qpe.Expression.SourceRef.Source = alias;
                                break;
                            default:
                                throw new NotImplementedException("Currently only support moving/renaming measures, columns and hierarchies");
                        }
                    }
                    sources.Add(new EntitySource() { Entity = kvp.Key, Name = alias });
                }
                SyncSources(sources, queryDefinition.From);
            }

            private void SyncSources(List<EntitySource> sources, List<EntitySource> from)
            {
                var srcDict = sources.ToDictionary(s => s.Entity, f => f);
                var fromDict = from.Where(f => f.Entity != null && f.Schema == null).ToDictionary(f => f.Entity, f => f);
                foreach (var s in sources)
                {
                    // adjust alias for existing source if needed
                    if (fromDict.TryGetValue(s.Entity, out var fromEntity))
                        fromEntity.Name = s.Name;
                    else
                        from.Add(s);
                }

                foreach (var f in fromDict.Values)
                {
                    if (!srcDict.ContainsKey(f.Entity))
                        from.Remove(f);
                }
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                Process(expression);
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                Process(expression);
            }

            protected override void Visit(QuerySourceRefExpression expression)
            {
                // skipping over the ones that have schema set, those are extensions (report-defined measures)
                // todo: check if it's possible that it's from the model. If it is possible, we need to add support for model schemas
                if (expression.Schema != null)
                    return;

                // for expressions that aren't using aliases, just replace the table name (if needed)
                if (expression.Entity != null)
                {
                    var oldName = expression.Entity;
                    var newName = renames.Table(expression.Entity).NewTableName;
                    if (newName != oldName)
                    {
                        expression.Entity = newName;
                        logger.LogInformation("Replaced a reference to table '{tableName}' with new name '{newName}'. (Path '{path}')", oldName, newName, Path);
                        incrementReplaceCountAction();
                    }
                }
            }

            Dictionary<string, List<QueryExpression>> aliasedPropertyExpressions = new Dictionary<string, List<QueryExpression>>();
            private void Process(QueryPropertyExpression expression)
            {
                base.Visit(expression);

                var sourceRef = expression.Expression.SourceRef;
                // skipping over the ones that have schema set, those are extensions (report-defined measures)
                // todo: check if it's possible that it's from the model. If it is possible, we need to add support for model schemas
                if (sourceRef == null || sourceRef.Schema != null)
                    return;

                if (sourceRef.Source != null)
                {
                    var originalOwnerTableName = SourcesByAliasMap[sourceRef.Source];

                    var tableMapping = renames.Table(originalOwnerTableName);
                    if (tableMapping.TryGetMapping(expression.Property, out var objectMapping))
                    {
                        var originalName = expression.Property;
                        var newName = objectMapping.NewObjectName;
                        expression.Property = newName;

                        aliasedPropertyExpressions
                            .GetValueOrInitialize(objectMapping.NewTableName, _ => new List<QueryExpression>())
                            .Add(expression);

                        logger.LogInformation("Replaced a reference to an object in table '{tableName}' from '{oldName}' to '{newName}'. (Path '{path}')", originalOwnerTableName, originalName, newName, Path);
                        incrementReplaceCountAction();
                    }
                    else
                    {
                        // use tableMapping.NewTableName because the owning table name might have been changed
                        aliasedPropertyExpressions
                            .GetValueOrInitialize(tableMapping.NewTableName, _ => new List<QueryExpression>())
                            .Add(expression);
                    }
                }
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {
                base.Visit(expression);

                var sourceRef = expression.Expression.SourceRef;
                if (sourceRef == null)
                    return;

                if (sourceRef.Source != null)
                {
                    var originalOwnerTableName = SourcesByAliasMap[sourceRef.Source];

                    if (renames.Table(originalOwnerTableName).TryGetMapping(expression.Hierarchy, out var objectMapping))
                    {
                        var originalName = expression.Hierarchy;
                        var newName = objectMapping.NewObjectName;
                        expression.Hierarchy = newName;

                        aliasedPropertyExpressions
                            .GetValueOrInitialize(objectMapping.NewTableName, _ => new List<QueryExpression>())
                            .Add(expression);

                        logger.LogInformation("Replaced a reference to an hierarchy in table '{tableName}' from '{oldName}' to '{newName}'. (Path '{path}')", originalOwnerTableName, originalName, newName, Path);
                        incrementReplaceCountAction();
                    }
                    else
                    {
                        aliasedPropertyExpressions
                            .GetValueOrInitialize(originalOwnerTableName, _ => new List<QueryExpression>())
                            .Add(expression);
                    }
                }
            }

            private string FindNextAvailableAlias(string v, HashSet<string> takenNames)
            {
                if (!takenNames.Contains(v))
                    return v;
                else
                {
                    for (int i = 1; ; i++)
                    {
                        string candidate = $"{v}{i}";
                        if (!takenNames.Contains(candidate))
                            return candidate;
                    }
                }
            }
        }
    }
}
