using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;
using System.Runtime.CompilerServices;

namespace Packer2.Library.Report.Transforms
{
    /*
    renames.ForTable("name"[, "newName"]).ForObject("old", "new").ForObject(...)
     */

    record Move(string oldTableName, string newTableName, string oldObjectName, string newObjectName);

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

        List<Move> moves = new List<Move>();
        public void AddMove(string oldTableName, string oldObjectName, string newTableName, string newObjectName)
        { 
            moves.Add(new Move(oldTableName, newTableName, oldObjectName, newObjectName));
        }

        public bool TryGetMove(string oldTableName, string oldObjectName, out string? newTableName, out string? newObjectName)
        {
            var m = moves.Where(m => m.oldTableName == oldTableName && m.oldObjectName == oldObjectName).SingleOrDefault();
            if (m == null)
            {
                newTableName = default;
                newObjectName = default;
                return false;
            }
            
            newTableName = m.newTableName;
            newObjectName = m.newObjectName;
            return true;
        }

        public IEnumerable<string> GetTablesMappedTo(string tableName)
        {
            return tableRenamesDict.Where(tn => tn.Value == tableName).Select(x => x.Key);
        }

        public Renames GetTableRenamesOnly()
            => new Renames() { tableRenamesDict = this.tableRenamesDict };

        public Renames GetNonTableRenamesOnly()
            => new Renames() { measureUpdatedOwnerTables = this.measureUpdatedOwnerTables, tableObjectsRenamesDict = this.tableObjectsRenamesDict, moves = this.moves };

        Dictionary<string, TableObjectRenames> tableObjectsRenamesDict = new Dictionary<string, TableObjectRenames>();
        Dictionary<string, string> measureUpdatedOwnerTables = new Dictionary<string, string>();
        Dictionary<string, string> tableRenamesDict = new Dictionary<string, string>();
        public void AddTableRename(string tableName, string newTableName)
        {
            tableRenamesDict[tableName] = newTableName;
        }

        public void AddRenames(string tableName, Dictionary<string, string> objectRenames)
        {
            foreach (var kvp in objectRenames)
                AddRename(tableName, kvp.Key, kvp.Value);
        }

        public void AddMeasureRebase(string measureName, string desiredOwnerTable)
        {
            measureUpdatedOwnerTables[measureName] = desiredOwnerTable;
        }

        public void AddRename(string tableName, string oldObjectName, string newObjectName)
        {
            if (!tableObjectsRenamesDict.TryGetValue(tableName, out var renames))
                renames = tableObjectsRenamesDict[tableName] = new TableObjectRenames();
            renames.Add(oldObjectName, newObjectName);
        }

        public bool TryGetTableRename(string tableName, out string? newName)
        {
            return tableRenamesDict.TryGetValue(tableName, out newName);
        }

        public bool TryGetTableObjectRename(string tableName, string objectName, out string? newName)
        {
            if (
                tableObjectsRenamesDict.TryGetValue(tableName, out var tableObjectRenames_inner)
                && tableObjectRenames_inner.TryGetNewName(objectName, out newName)
            )
                return true;
            else
            {
                newName = null;
                return false;
            }
        }

        public bool TryGetMeasureNewOwnerTable(string measureName, out string? newOwnerTable)
            => measureUpdatedOwnerTables.TryGetValue(measureName, out newOwnerTable);

        internal bool HasMoves()
            => moves.Count > 0;
    }

    public class ReplaceModelReferenceTransform : ReportInfoNavTransformBase
    {
        private readonly Renames renames;
        private readonly ILogger<ReplaceModelReferenceTransform> logger;

        int numberOfReplacements;

        public ReplaceModelReferenceTransform(Renames renames, ILogger<ReplaceModelReferenceTransform>? logger = null)
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

        FilterDefinition filterDefinition;
        protected override void ProcessFilter(FilterDefinition filterObj, string outerPath, string innerPath)
        {
            // todo: this seems dirty, check if it's possible to clean it up
            filterDefinition = filterObj;
            base.ProcessFilter(filterObj, outerPath, innerPath);
            ProcessFromSection(outerPath, innerPath, filterObj.From);
        }

        QueryDefinition queryDefinition;
        protected override void ProcessQuery(QueryDefinition expObj, string outerPath, string innerPath)
        {
            // todo: this seems dirty, check if it's possible to clean it up
            queryDefinition = expObj;
            base.ProcessQuery(expObj, outerPath, innerPath);
            ProcessFromSection(outerPath, innerPath, expObj.From);
        }

        private void ProcessFromSection(string outerPath, string innerPath, List<EntitySource> sources)
        {
            foreach (var f in sources)
            {
                if (f.Entity != null && renames.TryGetTableRename(f.Entity, out var newName))
                {
                    var oldName = f.Entity;
                    f.Entity = newName;
                    logger.LogInformation("Replaced a reference to table '{tableName}' with new name '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", oldName, newName, outerPath, innerPath);
                    numberOfReplacements++;
                }
            }
        }

        protected override QueryExpressionVisitor CreateProcessingVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap = null)
            => new ReplaceRefErrorsVisitor(outerPath, innerPath, sourceByAliasMap, renames, logger, () => numberOfReplacements++, queryDefinition, filterDefinition);

        class ReplaceRefErrorsVisitor : BaseQueryExpressionVisitor
        {
            private readonly Renames renames;
            private readonly ILogger logger;
            private readonly Action incrementReplaceCountAction;
            private readonly QueryDefinition queryDef;
            private readonly FilterDefinition filterDef;
            private readonly Dictionary<string, EntitySource>? fromSources;

            public ReplaceRefErrorsVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap, Renames renames, ILogger traceRefErrorReporter, Action incrementReplaceCountAction, QueryDefinition queryDef, FilterDefinition filterDef)
                : base(outerPath, innerPath, sourceByAliasMap)
            {
                this.renames = renames;
                this.logger = traceRefErrorReporter;
                this.incrementReplaceCountAction = incrementReplaceCountAction;
                this.queryDef = queryDef;
                this.filterDef = filterDef;
                this.fromSources = (queryDef?.From ?? filterDef?.From)?.ToDictionary(fs => fs.Name, fs => fs);
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                Process(expression);
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                Process(expression);

                if (renames.TryGetMeasureNewOwnerTable(expression.Property, out var newOwnerName))
                {
                    if (expression.Expression.Expression is QuerySourceRefExpression qse)
                    {
                        if (qse.Entity != null)
                        {
                            if (qse.Entity != newOwnerName)
                            {
                                logger.LogInformation("Rebased measure reference '{measure}' from table {oldTable} to table '{newTable}'. (Outer path '{outerPath}', inner path {innerPath})", expression.Property, qse.Entity, newOwnerName, OuterPath, InnerPath);
                                qse.Entity = newOwnerName;
                            }
                        }
                        else
                        {
                            // todo: implement via "move"
                            logger.LogWarning("Skipping rebase measure '{measure}' from table '{oldTable}' to table '{newTable}' because the table is aliased and automatic modification of the 'from' section is currently not supported. (Outer path '{outerPath}', inner path {innerPath})", expression.Property, qse.Entity, newOwnerName, OuterPath, InnerPath);
                        }
                    }
                    else
                    {
                        // todo: implement via "move"
                        logger.LogWarning("Skipping rebase measure '{measure}' to table '{newTable}' because the original table is a subquery and this case is currently not supported. (Outer path '{outerPath}', inner path {innerPath})", expression.Property, newOwnerName, OuterPath, InnerPath);
                    }
                }
            }

            protected override void Visit(QuerySourceRefExpression expression)
            {
                if (expression.Entity != null && renames.TryGetTableRename(expression.Entity, out var newName))
                {
                    var oldName = expression.Entity;
                    expression.Entity = newName;
                    logger.LogInformation("Replaced a reference to table '{tableName}' with new name '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", oldName, newName, OuterPath, InnerPath);
                    incrementReplaceCountAction();
                }
            }

            private void Process(QueryPropertyExpression expression)
            {
                var sourceRef = expression.Expression.SourceRef;
                if (sourceRef == null)
                    return;

                expression.Expression.Expression.Accept(this);

                var sourceName = sourceRef.Entity ?? SourcesByAliasMap[sourceRef.Source];

                if (renames.TryGetTableObjectRename(sourceName, expression.Property, out var newName))
                {
                    var originalName = expression.Property;
                    expression.Property = newName;
                    logger.LogInformation("Replaced a reference to an object in table '{tableName}' from '{oldName}' to '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", sourceName, originalName, newName, OuterPath, InnerPath);
                    incrementReplaceCountAction();
                }

                if (renames.TryGetMove(sourceName, expression.Property, out var newTableName, out var newObjectName))
                {
                    if (sourceRef.Entity != null)
                    {
                        sourceRef.Entity = newTableName;
                        expression.Property = newObjectName;
                    }
                    else
                    {
                        string alias;
                        if (fromSources.TryGetValue(newTableName!, out var es))
                        {
                            alias = SourcesByAliasMap.Where(x => x.Value == es.Name).Select(x => x.Key).Single();
                        }
                        else
                        {
                            alias = FindNextAvailableAlias(newTableName.ToLower().Substring(0,1), SourcesByAliasMap.Keys.ToHashSet());
                            var newEntitySource = new EntitySource() { Entity = newTableName, Name = alias };
                            SourcesByAliasMap.Add(alias, newTableName);
                            fromSources.Add(alias, newEntitySource);
                            var froms = queryDef.From ?? filterDef.From;
                            froms.Add(newEntitySource);

                            var visitor = new DetectTableAliasRefVisitor(sourceRef.Source);
                            if (queryDef != null)
                                visitor.Visit(queryDef);
                            else if (filterDef != null)
                                visitor.Visit(filterDef);
                            else
                                throw new NotImplementedException();

                            if (visitor.RefCount == 1)
                            {
                                froms.Remove(fromSources[sourceRef.Source]);
                            }
                        }
                        
                        sourceRef.Source = alias;
                        expression.Property = newObjectName;

                        // todo: we'll have to modify the from list
                    }
                }
            }

            // todo: use DefaultQueryDefinitionVisitor to replace "ReportInfoNavTransformBase"
            // ProcessQuery and ProcessFilter methods. It should have an inner expression visitor.
            // use it here as well to navigate the query or filter

            class DetectTableAliasRefVisitor : DefaultQueryDefinitionVisitor
            {
                private readonly DetectTableAliasExprVisitor expVisitor;

                public int RefCount => expVisitor.RefCount;

                public DetectTableAliasRefVisitor(string targetAliasName)
                {
                    this.expVisitor = new DetectTableAliasExprVisitor(targetAliasName);
                }

                protected override void VisitExpression(QueryExpressionContainer expression)
                {
                    expVisitor.VisitExpression(expression);
                }

                public void Visit(FilterDefinition filterDefinition)
                {
                    if (filterDefinition.From != null)
                    {
                        foreach (EntitySource item2 in filterDefinition.From)
                        {
                            VisitEntitySource(item2);
                        }
                    }

                    if (filterDefinition.Where != null)
                    {
                        foreach (QueryFilter item3 in filterDefinition.Where)
                        {
                            VisitFilter(item3);
                        }
                    }
                }

                class DetectTableAliasExprVisitor : DefaultQueryExpressionVisitor
                {
                    private readonly string targetAlias;

                    public int RefCount { get; private set; }

                    public DetectTableAliasExprVisitor(string targetAlias)
                    {
                        this.targetAlias = targetAlias;
                    }

                    protected override void Visit(QueryColumnExpression expression)
                    {
                        if (expression.Expression.SourceRef?.Source == targetAlias)
                            RefCount++;
                    }

                    protected override void Visit(QueryMeasureExpression expression)
                    {
                        if (expression.Expression.SourceRef?.Source == targetAlias)
                            RefCount++;
                    }
                }
            }

            private string FindNextAvailableAlias(string v, HashSet<string> takenNames)
            {
                if (!takenNames.Contains(v))
                    return v;
                else
                {
                    for(int i = 1; ;i++)
                    {
                        string candidate = $"{v}{i}";
                        if (!takenNames.Contains(candidate))
                            return candidate;
                    }
                }
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {
                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source];

                if (renames.TryGetTableObjectRename(sourceName, expression.Hierarchy, out var newName))
                {
                    var originalName = expression.Hierarchy;
                    expression.Hierarchy = newName;
                    logger.LogInformation("Replaced a reference to an object in table '{tableName}' from '{oldName}' to '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", sourceName, originalName, newName, OuterPath, InnerPath);
                    incrementReplaceCountAction();
                }
            }

            protected override void Visit(QueryHierarchyLevelExpression expression)
            {
                Visit(expression.Expression.Hierarchy);
                // we don't rename levels (should we?)
            }
        }
    }
}
