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
        private readonly ILogger<ReplaceModelReferenceTransform> logger;

        public ReplaceModelReferenceTransform(Renames renames, ILogger<ReplaceModelReferenceTransform>? logger = null) 
        {
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

        protected override BaseQueryExpressionVisitor Visitor => visitor;

        class ReplaceRefErrorsVisitor : BaseQueryExpressionVisitor
        {
            public int NumberOfReplacements { get; private set; } = 0;

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

            protected override void Visit(QueryHierarchyExpression expression)
            {
                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source];

                if (renames.TryGetRenames(sourceName, expression.Hierarchy, out var newName))
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
            }
        }
    }
}
