using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Microsoft.InfoNav.Explore.VisualContracts;
using Newtonsoft.Json;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Transforms
{
    public class ValidateModelReferencesTransform : IReportTransform
    {
        private readonly ILogger<ValidateModelReferencesTransform> logger;
        private readonly Database database;

        // todo: replace with logger?
        public ValidateModelReferencesTransform(Database database, ILogger<ValidateModelReferencesTransform> logger = null)
        {
            this.logger = logger ?? new DummyLogger<ValidateModelReferencesTransform>();
            this.database = database;
        }

        public PowerBIReport Transform(PowerBIReport model)
        {
            int invalidRefsCount = 0;

            foreach (var sectionJObj in model.Layout.SelectTokens(".sections[*]").ToArray())
            {
                var section = $"{sectionJObj["displayName"]} <{sectionJObj["name"]}>";
                foreach (var visualContainerJObj in sectionJObj.SelectTokens(".visualContainers[*]").ToArray())
                {
                    
                    var visualContainerConfig = JsonConvert.DeserializeObject<VisualContainerConfig>(visualContainerJObj["config"].ToString())!;

                    if (visualContainerConfig.SingleVisual?.PrototypeQuery == null)
                        continue;

                    var sourcesToIgnore = visualContainerConfig.SingleVisual.PrototypeQuery.From.Where(f => f.Type != EntitySourceType.Table).Select(f => f.Name).ToHashSet();
                    var sourcesToCheck = visualContainerConfig.SingleVisual.PrototypeQuery.From.Where(f => f.Type == EntitySourceType.Table).ToDictionary(f => f.Name, f => f.Entity);

                    var refErrorsDetectVisitor = new DetectRefErrorsVisitor(section, visualContainerConfig.Name, sourcesToIgnore, sourcesToCheck, database, logger);
                    foreach (var s in visualContainerConfig.SingleVisual.PrototypeQuery.Select)
                        s.Expression.Accept(refErrorsDetectVisitor);

                    invalidRefsCount += refErrorsDetectVisitor.MissingSourceErrors.Count() + refErrorsDetectVisitor.MissingMeasureErrors.Count() + refErrorsDetectVisitor.MissingSourceErrors.Count();
                }
            }
            if (invalidRefsCount > 0)
                throw new Exception($"Found {invalidRefsCount} invalid data model references!");

            return model;
        }

        public record MissingSourceError(string sourceName, string sourceAlias);
        public record MissingColumnError(string sourceName, string sourceAlias, string columnName);
        public record MissingMeasureError(string sourceName, string sourceAlias, string columnName);

        class DetectRefErrorsVisitor : QueryExpressionVisitor
        {
            List<MissingSourceError> missingSourceErrors = new List<MissingSourceError>();
            List<MissingColumnError> missingColumnsErrors = new List<MissingColumnError>();
            List<MissingMeasureError> missingMeasureErrors = new List<MissingMeasureError>();
            private readonly string sectionName;
            private readonly string visualName;
            private readonly HashSet<string> sourcesToIgnore;
            private readonly IDictionary<string, string> sourcesToCheck;
            private readonly Database db;
            private readonly ILogger traceRefErrorReporter;

            public IEnumerable<MissingSourceError> MissingSourceErrors { get => missingSourceErrors; }
            public IEnumerable<MissingColumnError> MissingColumnsErrors { get => missingColumnsErrors; }
            public IEnumerable<MissingMeasureError> MissingMeasureErrors { get => missingMeasureErrors; }

            private void RegisterMissingSource(string name, string alias)
            {
                var error = new MissingSourceError(name, alias);
                missingSourceErrors.Add(error);
                traceRefErrorReporter.LogError("Page '{page}' - visual '{visual}' references data source '{dataSourceName}' with alias '{dataSourceAlias}' which was not found in the database.", sectionName, visualName, name, alias);
            }

            private void RegisterMissingColumn(string name, string alias, string columnName)
            {
                var error = new MissingColumnError(name, alias, columnName);
                missingColumnsErrors.Add(error);
                traceRefErrorReporter.LogError("Page '{page}' - visual '{visual}' references column '{columnName}' which was not found in data source '{dataSourceName}' with alias '{dataSourceAlias}'", sectionName, visualName, columnName, name, alias);
            }

            private void RegisterMissingMeasure(string name, string alias, string measureName)
            {
                var error = new MissingMeasureError(name, alias, measureName);
                missingMeasureErrors.Add(error);
                traceRefErrorReporter.LogError("Page '{page}' - visual '{visual}' references measure '{columnName}' which was not found in data source '{dataSourceName}' with alias '{dataSourceAlias}'", sectionName, visualName, measureName, name, alias);
            }

            public DetectRefErrorsVisitor(string sectionName, string visualName, HashSet<string> sourcesToIgnore, IDictionary<string, string> sourcesToCheck, Database db, ILogger traceRefErrorReporter)
            {
                this.db = db;
                this.sectionName = sectionName;
                this.visualName = visualName;
                this.sourcesToIgnore = sourcesToIgnore;
                this.sourcesToCheck = sourcesToCheck;
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

            private void ProcessCollection<T>(string name, string sourceAlias, Func<Table, NamedMetadataObjectCollection<T, Table>> targetCollectionGetter, Action<string, string, string> reportInvalidReference)
                where T : NamedMetadataObject
            {
                if (!sourcesToIgnore.Contains(sourceAlias))
                {
                    var sourceName = sourcesToCheck[sourceAlias];
                    if (db.Model.Tables.Contains(sourceName))
                    {
                        var table = db.Model.Tables[sourceName];
                        var tagetCollection = targetCollectionGetter(table);
                        if (!tagetCollection.Contains(name))
                        {
                            reportInvalidReference(sourceName, sourceAlias, name);
                        }
                    }
                    else
                    {
                        RegisterMissingSource(sourceName, sourceAlias);
                    }
                }
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                ProcessCollection(
                    expression.Property,
                    expression.Expression.SourceRef.Source,
                    t => t.Columns,
                    RegisterMissingColumn);
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                ProcessCollection(
                    expression.Property,
                    expression.Expression.SourceRef.Source,
                    t => t.Measures,
                    RegisterMissingMeasure);
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {

            }

            protected override void Visit(QueryHierarchyLevelExpression expression)
            {

            }

            protected override void Visit(QueryPropertyVariationSourceExpression expression)
            {

            }

            protected override void Visit(QueryAggregationExpression expression)
            {
            }

            protected override void Visit(QueryDatePartExpression expression)
            {

            }

            protected override void Visit(QueryPercentileExpression expression)
            {

            }

            protected override void Visit(QueryFloorExpression expression)
            {

            }

            protected override void Visit(QueryDiscretizeExpression expression)
            {

            }

            protected override void Visit(QueryMemberExpression expression)
            {

            }

            protected override void Visit(QueryNativeFormatExpression expression)
            {

            }

            protected override void Visit(QueryNativeMeasureExpression expression)
            {

            }

            protected override void Visit(QueryExistsExpression expression)
            {

            }

            protected override void Visit(QueryNotExpression expression)
            {

            }

            protected override void Visit(QueryAndExpression expression)
            {

            }

            protected override void Visit(QueryOrExpression expression)
            {

            }

            protected override void Visit(QueryComparisonExpression expression)
            {

            }

            protected override void Visit(QueryContainsExpression expression)
            {

            }

            protected override void Visit(QueryStartsWithExpression expression)
            {

            }

            protected override void Visit(QueryArithmeticExpression expression)
            {

            }

            protected override void Visit(QueryEndsWithExpression expression)
            {

            }

            protected override void Visit(QueryBetweenExpression expression)
            {

            }

            protected override void Visit(QueryInExpression expression)
            {

            }

            protected override void Visit(QueryScopedEvalExpression expression)
            {

            }

            protected override void Visit(QueryFilteredEvalExpression expression)
            {

            }

            protected override void Visit(QuerySparklineDataExpression expression)
            {

            }

            protected override void Visit(QueryBooleanConstantExpression expression)
            {

            }

            protected override void Visit(QueryDateConstantExpression expression)
            {

            }

            protected override void Visit(QueryDateTimeConstantExpression expression)
            {

            }

            protected override void Visit(QueryDateTimeSecondConstantExpression expression)
            {

            }

            protected override void Visit(QueryDecadeConstantExpression expression)
            {

            }

            protected override void Visit(QueryDecimalConstantExpression expression)
            {

            }

            protected override void Visit(QueryIntegerConstantExpression expression)
            {

            }

            protected override void Visit(QueryNullConstantExpression expression)
            {

            }

            protected override void Visit(QueryStringConstantExpression expression)
            {

            }

            protected override void Visit(QueryNumberConstantExpression expression)
            {

            }

            protected override void Visit(QueryYearAndMonthConstantExpression expression)
            {

            }

            protected override void Visit(QueryYearAndWeekConstantExpression expression)
            {

            }

            protected override void Visit(QueryYearConstantExpression expression)
            {

            }

            protected override void Visit(QueryLiteralExpression expression)
            {

            }

            protected override void Visit(QueryDefaultValueExpression expression)
            {

            }

            protected override void Visit(QueryAnyValueExpression expression)
            {

            }

            protected override void Visit(QueryNowExpression expression)
            {

            }

            protected override void Visit(QueryDateAddExpression expression)
            {

            }

            protected override void Visit(QueryDateSpanExpression expression)
            {

            }

            protected override void Visit(QueryTransformOutputRoleRefExpression expression)
            {

            }

            protected override void Visit(QueryTransformTableRefExpression expression)
            {

            }

            protected override void Visit(QuerySubqueryExpression expression)
            {

            }

            protected override void Visit(QueryLetRefExpression expression)
            {

            }

            protected override void Visit(QueryRoleRefExpression expression)
            {

            }

            protected override void Visit(QuerySummaryValueRefExpression expression)
            {

            }

            protected override void Visit(QueryParameterRefExpression expression)
            {

            }

            protected override void Visit(QueryTypeOfExpression expression)
            {

            }

            protected override void Visit(QueryPrimitiveTypeExpression expression)
            {

            }

            protected override void Visit(QueryTableTypeExpression expression)
            {

            }
        }
    }
}
