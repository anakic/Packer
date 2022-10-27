using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Transforms
{
    public class ValidateModelReferencesTransform : ModelReferenceTransformBase
    {
        private readonly DetectRefErrorsVisitor visitor;
        private readonly ILogger<ValidateModelReferencesTransform> logger;

        // todo: replace with logger?
        public ValidateModelReferencesTransform(Database database, ILogger<ValidateModelReferencesTransform>? logger = null)
        {
            this.logger = logger ?? new DummyLogger<ValidateModelReferencesTransform>();
            visitor = new DetectRefErrorsVisitor(database, this.logger);
        }

        protected override BaseQueryExpressionVisitor Visitor => visitor;

        protected override void OnProcessingComplete(PowerBIReport model)
        {
            var totalErrors = visitor.MissingSourceErrors.Count() + 
                visitor.MissingColumnsErrors.Count() + 
                visitor.MissingMeasureErrors.Count();

            if (totalErrors == 0)
                logger.LogInformation("No validation errors detected");
            else
                throw new Exception($"A total of {totalErrors} validation errors have been detected.");
        }

        public record MissingSourceError(string sourceName);
        public record MissingColumnError(string sourceName, string columnName);
        public record MissingMeasureError(string sourceName, string columnName);

        class DetectRefErrorsVisitor : BaseQueryExpressionVisitor
        {
            List<MissingSourceError> missingSourceErrors = new List<MissingSourceError>();
            List<MissingColumnError> missingColumnsErrors = new List<MissingColumnError>();
            List<MissingMeasureError> missingMeasureErrors = new List<MissingMeasureError>();

            public HashSet<string>? SourcesToIgnore { get; set; } = new HashSet<string>();

            private readonly Database db;
            private readonly ILogger traceRefErrorReporter;

            public IEnumerable<MissingSourceError> MissingSourceErrors { get => missingSourceErrors; }
            public IEnumerable<MissingColumnError> MissingColumnsErrors { get => missingColumnsErrors; }
            public IEnumerable<MissingMeasureError> MissingMeasureErrors { get => missingMeasureErrors; }

            private void RegisterMissingSource(string sourceName)
            {
                var error = new MissingSourceError(sourceName);
                missingSourceErrors.Add(error);
                traceRefErrorReporter.LogError("Invalid data source reference '{dataSourceName}' found at path '{outerPath}' => '{innerPath}'.", sourceName, OuterPath, InnerPath);
            }

            private void RegisterMissingColumn(string sourceName, string columnName)
            {
                var error = new MissingColumnError(sourceName, columnName);
                missingColumnsErrors.Add(error);
                traceRefErrorReporter.LogError("Invalid column reference '{columnName}' in data source '{dataSourceName}' found at path '{outerPath}' => '{innerPath}'", columnName, sourceName, OuterPath, InnerPath);
            }

            private void RegisterMissingMeasure(string sourceName, string measureName)
            {
                var error = new MissingMeasureError(sourceName, measureName);
                missingMeasureErrors.Add(error);
                traceRefErrorReporter.LogError("Invalid measure reference '{columnName}' in data source '{dataSourceName}' found at path '{outerPath}' => '{innerPath}'", measureName, sourceName, OuterPath, InnerPath);
            }

            public DetectRefErrorsVisitor(Database db, ILogger traceRefErrorReporter)
            {
                this.db = db;
                this.traceRefErrorReporter = traceRefErrorReporter;
            }

            protected override void Visit(QuerySourceRefExpression expression)
            {
                // todo: implement
            }

            protected override void Visit(QueryPropertyExpression expression)
            {
                // todo: implement
            }

            private void ProcessCollection<T>(string name, string sourceName, Func<Table, NamedMetadataObjectCollection<T, Table>> targetCollectionGetter, Action<string, string> reportInvalidReference)
                where T : NamedMetadataObject
            {
                if (!SourcesToIgnore.Contains(sourceName))
                {
                    if (db.Model.Tables.Contains(sourceName))
                    {
                        var table = db.Model.Tables[sourceName];
                        var tagetCollection = targetCollectionGetter(table);
                        if (!tagetCollection.Contains(name))
                            reportInvalidReference(sourceName, name);
                    }
                    else
                    {
                        RegisterMissingSource(sourceName);
                    }
                }
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {
            }

            protected override void Visit(QueryHierarchyLevelExpression expression)
            {
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                ProcessCollection(
                    expression.Property,
                    expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source],
                    t => t.Columns,
                    RegisterMissingColumn);
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                ProcessCollection(
                    expression.Property,
                    expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source],
                    t => t.Measures,
                    RegisterMissingMeasure);
            }
        }
    }
}
