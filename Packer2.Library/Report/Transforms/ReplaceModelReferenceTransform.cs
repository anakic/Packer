using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
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

        protected override void ProcessFilter(FilterDefinition filterObj, string outerPath, string innerPath)
        {
            base.ProcessFilter(filterObj, outerPath, innerPath);
            ProcessFromSection(outerPath, innerPath, filterObj.From);
        }

        protected override void ProcessQuery(QueryDefinition expObj, string outerPath, string innerPath)
        {
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
            => new ReplaceRefErrorsVisitor(outerPath, innerPath, sourceByAliasMap, renames, logger, () => numberOfReplacements++);

        class ReplaceRefErrorsVisitor : BaseQueryExpressionVisitor
        {
            private readonly Renames renames;
            private readonly ILogger logger;
            private readonly Action incrementReplaceCountAction;

            public ReplaceRefErrorsVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap, Renames renames, ILogger traceRefErrorReporter, Action incrementReplaceCountAction)
                : base(outerPath, innerPath, sourceByAliasMap)
            {
                this.renames = renames;
                this.logger = traceRefErrorReporter;
                this.incrementReplaceCountAction = incrementReplaceCountAction;
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
                            logger.LogWarning("Skipping rebase measure '{measure}' from table '{oldTable}' to table '{newTable}' because the table is aliased and automatic modification of the 'from' section is currently not supported. (Outer path '{outerPath}', inner path {innerPath})", expression.Property, qse.Entity, newOwnerName, OuterPath, InnerPath);
                        }
                    }
                    else
                    {
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
                if (expression.Expression.SourceRef == null)
                    return;

                expression.Expression.Expression.Accept(this);

                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source];

                if (renames.TryGetTableObjectRename(sourceName, expression.Property, out var newName))
                {
                    var originalName = expression.Property;
                    expression.Property = newName;
                    logger.LogInformation("Replaced a reference to an object in table '{tableName}' from '{oldName}' to '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", sourceName, originalName, newName, OuterPath, InnerPath);
                    incrementReplaceCountAction();
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
