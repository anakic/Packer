using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
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
    }

    public class ReplaceModelReferenceTransform : ModelReferenceTransformBase
    {
        ReplaceRefErrorsVisitor visitor;
        private readonly Renames renames;
        private readonly ILogger<ReplaceModelReferenceTransform> logger;

        public ReplaceModelReferenceTransform(Renames renames, ILogger<ReplaceModelReferenceTransform>? logger = null)
        {
            this.renames = renames;
            this.logger = logger ?? new DummyLogger<ReplaceModelReferenceTransform>();
            visitor = new ReplaceRefErrorsVisitor(renames, this.logger);
        }

        protected override void OnProcessingComplete(PowerBIReport model)
        {
            if (visitor.NumberOfReplacements == 0)
                throw new ArgumentException("No references were found, nothing to replace");
            else
                logger.LogInformation($"A total of {visitor.NumberOfReplacements} replacements were made.");
        }

        protected override void VisitQuery(QueryDefinition expObj, string outerPath, string innerPath)
        {
            base.VisitQuery(expObj, outerPath, innerPath);
            foreach (var f in expObj.From)
            {
                if (f.Entity != null && renames.TryGetTableRename(f.Entity, out var newName))
                {
                    var oldName = f.Entity;
                    f.Entity = newName;
                    logger.LogInformation("Replaced a reference to table '{tableName}' with new name '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", oldName, newName, outerPath, innerPath);
                    visitor.NumberOfReplacements++;
                }
            }
        }

        protected override BaseQueryExpressionVisitor Visitor => visitor;

        class ReplaceRefErrorsVisitor : BaseQueryExpressionVisitor
        {
            public int NumberOfReplacements { get; set; } = 0;

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

            protected override void Visit(QuerySourceRefExpression expression)
            {
                if (expression.Entity != null && renames.TryGetTableRename(expression.Entity, out var newName))
                {
                    var oldName = expression.Entity;
                    expression.Entity = newName;
                    logger.LogInformation("Replaced a reference to table '{tableName}' with new name '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", oldName, newName, OuterPath, InnerPath);
                    NumberOfReplacements++;
                }
            }

            private void Process(QueryPropertyExpression expression)
            {
                expression.Expression.Expression.Accept(this);

                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source];

                if (renames.TryGetTableObjectRename(sourceName, expression.Property, out var newName))
                {
                    var originalName = expression.Property;
                    expression.Property = newName;
                    logger.LogInformation("Replaced a reference to an object in table '{tableName}' from '{oldName}' to '{newName}'. (Outer path '{outerPath}', inner path {innerPath})", sourceName, originalName, newName, OuterPath, InnerPath);
                    NumberOfReplacements++;
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
                    NumberOfReplacements++;
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
