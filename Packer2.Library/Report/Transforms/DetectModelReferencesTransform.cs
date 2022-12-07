using Microsoft.AnalysisServices.Tabular;
using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Packer2.Library.Report.Transforms
{
    public record DetectedTableReference(string outerPath, string innerPath, string TableName);

    public record DetectedColumnReference(string outerPath, string innerPath, string TableName, string column) 
        : DetectedTableReference(outerPath, innerPath, TableName);

    public record DetectedMeasureReference(string outerPath, string innerPath, string TableName, string measure)
        : DetectedTableReference(outerPath, innerPath, TableName);

    public record DetectedHierarchyReference(string outerPath, string innerPath, string TableName, string hierarchy)
        : DetectedTableReference(outerPath, innerPath, TableName);

    public record DetectedHierarchyLevelReference(string outerPath, string innerPath, string TableName, string hierarchy, string level)
        : DetectedHierarchyReference(outerPath, innerPath, TableName, hierarchy);

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

        class DetectVisitor : DefaultQueryExpressionVisitor
        {
            private readonly Detections detections;
            private string outerPath;
            private string innerPath;
            private Dictionary<string, string> sourceByAliasMap;

            public DetectVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap, Detections detections)
            {
                this.outerPath = outerPath;
                this.innerPath = innerPath;
                this.sourceByAliasMap = sourceByAliasMap;
                this.detections = detections;
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                var sourceName = expression.Expression.SourceRef.Entity ?? sourceByAliasMap[expression.Expression.SourceRef.Source];
                detections.MeasureReferences.Add(new DetectedMeasureReference(outerPath, innerPath, sourceName, expression.Property));
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                if (expression.Expression.SourceRef == null)
                    return;

                var sourceName = expression.Expression.SourceRef.Entity ?? sourceByAliasMap[expression.Expression.SourceRef.Source];
                detections.ColumnReferences.Add(new DetectedColumnReference(outerPath, innerPath, sourceName, expression.Property));
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {
                if (expression.Expression.SourceRef == null)
                    return;

                var sourceName = expression.Expression.SourceRef.Entity ?? sourceByAliasMap[expression.Expression.SourceRef.Source];
                detections.HierarchyReferences.Add(new DetectedHierarchyReference(outerPath, innerPath, sourceName, expression.Hierarchy));
            }

            protected override void Visit(QueryHierarchyLevelExpression expression)
            {
                var sourceName = expression.Expression.SourceRef.Entity ?? sourceByAliasMap[expression.Expression.SourceRef.Source];
                // detections.HierarchyReferences.Add(new DetectedHierarchyLevelReference(outerPath, innerPath, sourceName, expression.Level, expression.));
            }
        }

        public DetectModelReferencesTransform(Detections detections)
        {
            this.detections = detections;
        }

        protected override QueryExpressionVisitor CreateProcessingVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap = null)
        {
            return new DetectVisitor(outerPath, innerPath, sourceByAliasMap, detections);
        }
    }
}
