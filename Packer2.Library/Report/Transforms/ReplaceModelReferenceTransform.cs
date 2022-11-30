using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Transforms
{
    public record TableObjectName(string TableName, string ObjectName);

    public class Renames
    {
        Dictionary<TableObjectName, TableObjectName> tableObjectsRenamesDict = new Dictionary<TableObjectName, TableObjectName>();

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

        public void AddRename(string tableName, string oldObjectName, string newObjectName)
            => AddRename(tableName, oldObjectName, newObjectName);

        public void AddRename(string originalParentTable, string oldObjectName, string newParentTable, string newObjectName)
        {
            var original = new TableObjectName(originalParentTable, oldObjectName);
            var renamed = new TableObjectName(newParentTable, newObjectName);
            tableObjectsRenamesDict[original] = renamed;
        }

        public bool TryGetTableRename(string tableName, out string? newName)
        {
            return tableRenamesDict.TryGetValue(tableName, out newName);
        }

        public bool TryGetTableObjectRename(string tableName, string objectName, out TableObjectName? renamed)
        {
            var original = new TableObjectName(tableName, objectName);
            return tableObjectsRenamesDict.TryGetValue(original, out renamed);
        }
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
                throw new ArgumentException("No references were found, nothing to replace");
            else
                logger.LogInformation($"A total of {numberOfReplacements} replacements were made.");
        }

        protected override void ProcessQuery(QueryDefinition expObj, string outerPath, string innerPath)
        {
            base.ProcessQuery(expObj, outerPath, innerPath);
            foreach (var f in expObj.From)
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
