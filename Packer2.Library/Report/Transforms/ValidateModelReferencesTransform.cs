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
            var sourcesToIgnore = expObj.From.Where(f => f.Type != EntitySourceType.Table).Select(f => f.Name).ToHashSet();

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
            public string OuterPath { get; set; }
            public string InnerPath { get; set; }

            List<MissingSourceError> missingSourceErrors = new List<MissingSourceError>();
            List<MissingColumnError> missingColumnsErrors = new List<MissingColumnError>();
            List<MissingMeasureError> missingMeasureErrors = new List<MissingMeasureError>();

            public HashSet<string>? SourcesToIgnore { get; set; } = new HashSet<string>();
            public IDictionary<string, string>? SourcesByAliasMap { get; set; }

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
                base.Visit(expression);
            }

            protected override void Visit(QueryHierarchyLevelExpression expression)
            {
                base.Visit(expression);
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
