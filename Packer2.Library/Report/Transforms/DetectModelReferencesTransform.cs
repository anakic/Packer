using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.Report.Queries;

namespace Packer2.Library.Report.Transforms
{
    public record DetectedTableReference(string path, string TableName);

    public record DetectedColumnReference(string path, string TableName, string column) 
        : DetectedTableReference(path, TableName);

    public record DetectedMeasureReference(string path, string TableName, string measure)
        : DetectedTableReference(path, TableName);

    public record DetectedHierarchyReference(string path, string TableName, string hierarchy)
        : DetectedTableReference(path, TableName);

    public record DetectedHierarchyLevelReference(string path, string TableName, string hierarchy, string level)
        : DetectedHierarchyReference(path, TableName, hierarchy);

    public class Detections
    {
        public List<DetectedTableReference> TableReferences { get; }
        public List<DetectedColumnReference> ColumnReferences { get; }
        public List<DetectedMeasureReference> MeasureReferences { get; }
        public List<DetectedHierarchyReference> HierarchyReferences { get; }
        public List<DetectedHierarchyLevelReference> HierarchyLevelReferences { get; }

        public Detections()
        {
            TableReferences = new List<DetectedTableReference>();
            ColumnReferences = new List<DetectedColumnReference>();
            MeasureReferences = new List<DetectedMeasureReference>();
            HierarchyReferences = new List<DetectedHierarchyReference>();
            HierarchyLevelReferences = new List<DetectedHierarchyLevelReference>();
        }

        public void Exclude(IEnumerable<Table> modelExtensions)
        {
            foreach (var meas in modelExtensions.SelectMany(t => t.Measures))
            {
                MeasureReferences
                    .Where(mr => mr.TableName == meas.Table.Name && mr.measure == meas.Name)
                    .ToList()
                    .ForEach(m => MeasureReferences.Remove(m));
            }
        }

        public void Add(Detections detections)
        {
            TableReferences.AddRange(detections.TableReferences);
            ColumnReferences.AddRange(detections.ColumnReferences);
            MeasureReferences.AddRange(detections.MeasureReferences);
            HierarchyReferences.AddRange(detections.HierarchyReferences);
            HierarchyLevelReferences.AddRange(detections.HierarchyLevelReferences);
        }
    }

    public class DetectModelReferencesTransform : ReportInfoNavTransformBase
    {
        private readonly Detections detections;

        class DetectVisitor : BaseTransformVisitor
        {
            private readonly Detections detections;

            public DetectVisitor(string path, Detections detections)
                :base(path)
            {
                this.detections = detections;
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source].Entity;
                detections.MeasureReferences.Add(new DetectedMeasureReference(Path, sourceName, expression.Property));
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                if (expression.Expression.SourceRef == null)
                    return;

                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source].Entity;
                detections.ColumnReferences.Add(new DetectedColumnReference(Path, sourceName, expression.Property));
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {
                if (expression.Expression.SourceRef == null)
                    return;

                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source].Entity;
                detections.HierarchyReferences.Add(new DetectedHierarchyReference(Path, sourceName, expression.Hierarchy));
            }
        }

        public DetectModelReferencesTransform(Detections detections, ILogger? logger = null)
            : base(logger)
        {
            this.detections = detections;
        }

        protected override ExtendedExpressionVisitor CreateProcessingVisitor(string path)
        {
            return new DetectVisitor(path, detections);
        }
    }
}
