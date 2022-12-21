using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.Report.Transforms
{
    public record InvalidHierarchyLevelDetection(string Path, string Table, string? Hierarchy = null, string? HierarchyLevel = null) : InvalidRefDetection(Path, Table);

    public record InvalidHierarchyDetection(string Path, string Table, string? Hierarchy = null) : InvalidRefDetection(Path, Table);

    public record InvalidMeasureDetection(string Path, string Table, string? Measure = null) : InvalidRefDetection(Path, Table);

    public record InvalidColumnDetection(string Path, string Table, string? Column = null) : InvalidRefDetection(Path, Table);

    public record InvalidTableDetection(string Path, string Table) 
        : InvalidRefDetection(Path, Table);

    public abstract record InvalidRefDetection(string Path, string Table);

    // todo: add unit tests for detecting invalid references
    public class ValidateModelReferencesTransform : ReportInfoNavTransformBase
    {
        private readonly Database database;
        List<InvalidRefDetection> unmatchedDetections = new List<InvalidRefDetection>();
        public IEnumerable<InvalidRefDetection> UnmatchedDetections => unmatchedDetections;

        public ValidateModelReferencesTransform(Database database, ILogger<ValidateModelReferencesTransform>? logger = null)
            : base(logger)
        {
            this.database = database;
        }

        protected override void Process(FilterDefinition filter, string path)
        {
            var detections = filter.DetectReferences();
            ProcessDetections(detections, path);
        }

        protected override void Process(QueryDefinition query, string path)
        {
            var detections = query.DetectReferences();
            ProcessDetections(detections, path);
        }

        protected override void Process(QueryExpressionContainer expression, string path)
        {
            var detections = expression.DetectReferences();
            ProcessDetections(detections, path);
        }

        private void ProcessDetections(Detections detections, string path)
        {
            foreach (var tbl in detections.TableReferences.GroupBy(x => x.TableName))
            {
                if (!database.Model.Tables.Contains(tbl.Key))
                    unmatchedDetections.Add(new InvalidTableDetection(path, tbl.Key));
            }

            foreach (var tbl in detections.ColumnReferences.GroupBy(x => new { x.TableName, x.Column }))
            {
                if (database.Model.Tables.Contains(tbl.Key.TableName))
                {
                    var t = database.Model.Tables[tbl.Key.TableName];
                    if(!t.Columns.Contains(tbl.Key.Column))
                        unmatchedDetections.Add(new InvalidColumnDetection(path, tbl.Key.TableName, tbl.Key.Column));
                }
            }

            foreach (var tbl in detections.MeasureReferences.GroupBy(x => new { x.TableName, x.Measure }))
            {
                if (database.Model.Tables.Contains(tbl.Key.TableName))
                {
                    var t = database.Model.Tables[tbl.Key.TableName];
                    if (!t.Measures.Contains(tbl.Key.Measure))
                        unmatchedDetections.Add(new InvalidMeasureDetection(path, tbl.Key.TableName, tbl.Key.Measure));
                }
            }

            foreach (var tbl in detections.HierarchyReferences.GroupBy(x => new { x.TableName, x.Hierarchy }))
            {
                if (database.Model.Tables.Contains(tbl.Key.TableName))
                {
                    var t = database.Model.Tables[tbl.Key.TableName];
                    if (!t.Hierarchies.Contains(tbl.Key.Hierarchy))
                        unmatchedDetections.Add(new InvalidHierarchyDetection(path, tbl.Key.TableName, tbl.Key.Hierarchy));
                }
            }

            foreach (var tbl in detections.HierarchyLevelReferences.GroupBy(x => new { x.TableName, x.Hierarchy, x.Level }))
            {
                if (database.Model.Tables.Contains(tbl.Key.TableName))
                {
                    var t = database.Model.Tables[tbl.Key.TableName];
                    if (t.Hierarchies.Contains(tbl.Key.Hierarchy))
                    {
                        var h = t.Hierarchies[tbl.Key.Hierarchy];
                        if (!h.Levels.Contains(tbl.Key.Level))
                        {
                            unmatchedDetections.Add(new InvalidHierarchyLevelDetection(path, tbl.Key.TableName, tbl.Key.Hierarchy, tbl.Key.Level));
                        }
                    }
                }
            }
        }
    }
}
