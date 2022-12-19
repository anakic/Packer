using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.Report.Queries;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Transforms
{
    public record DetectedTableReference(string TableName);

    public record DetectedColumnReference(string TableName, string Column)
        : DetectedTableReference(TableName);

    public record DetectedMeasureReference(string TableName, string Measure)
        : DetectedTableReference(TableName);

    public record DetectedHierarchyReference(string TableName, string Hierarchy)
        : DetectedTableReference(TableName);

    public record DetectedHierarchyLevelReference(string TableName, string Hierarchy, string Level)
        : DetectedHierarchyReference(TableName, Hierarchy);

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
                    .Where(mr => mr.TableName == meas.Table.Name && mr.Measure == meas.Name)
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

        public DetectModelReferencesTransform(Detections detections, ILogger? logger = null)
            : base(logger)
        {
            this.detections = detections;
        }

        protected override void Process(FilterDefinition filter, string path)
        {
            filter.DetectReferences(detections);
        }

        protected override void Process(QueryDefinition query, string path)
        {
            query.DetectReferences(detections);
        }

        protected override void Process(QueryExpressionContainer expression, string path)
        {
            expression.DetectReferences(detections);
        }
    }

    public static class QueryModelExtensions_Detect
    {
        public static void DetectReferences(this QueryDefinition queryDefinition, Detections detections)
        {
            new DetectVisitor(detections).Visit(queryDefinition);
        }

        public static void DetectReferences(this FilterDefinition filterDefinition, Detections detections)
        {
            new DetectVisitor(detections).Visit(filterDefinition);
        }

        public static void DetectReferences(this QueryExpressionContainer expressionContainer, Detections detections)
            => DetectReferences(expressionContainer.Expression, detections);


        public static void DetectReferences(this QueryExpression expression, Detections detections)
        {
            new DetectVisitor(detections).VisitExpression(expression);
        }

        class DetectVisitor : BaseTransformVisitor
        {
            private readonly Detections detections;

            public DetectVisitor(Detections detections)
            {
                this.detections = detections;
            }

            protected override void VisitEntitySource(EntitySource source)
            {
                base.VisitEntitySource(source);

                // skipping extensins (data model tables do not have a schema)
                if (source.Schema != null)
                    return;

                detections.TableReferences.Add(new DetectedTableReference(source.Entity));
            }

            protected override void Visit(QuerySourceRefExpression expression)
            {
                // skipping extensins (data model tables do not have a schema)
                if (expression.Schema != null)
                    return;

                if (expression.Entity != null)
                    detections.TableReferences.Add(new DetectedTableReference(expression.Entity));
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                if (expression.Expression.SourceRef == null)
                    return;

                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source].Entity;
                detections.MeasureReferences.Add(new DetectedMeasureReference(sourceName, expression.Property));
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                if (expression.Expression.SourceRef == null)
                    return;

                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source].Entity;
                detections.ColumnReferences.Add(new DetectedColumnReference(sourceName, expression.Property));
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {
                if (expression.Expression.SourceRef == null)
                    return;

                var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source].Entity;
                detections.HierarchyReferences.Add(new DetectedHierarchyReference(sourceName, expression.Hierarchy));
            }
        }
    }
}
