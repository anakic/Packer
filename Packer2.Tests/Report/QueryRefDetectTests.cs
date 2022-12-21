using FluentAssertions;
using Packer2.Library.Report.QueryTransforms.Antlr;
using Packer2.Library.Report.Transforms;
using Packer2.Tests.Tools;
using Xunit;

namespace Packer2.Tests.Report
{
    public class QueryRefDetectTests
    {
        QueryParser parser = new QueryParser(new TestDbInfoGetter());
        
        [Fact]
        public void DetectsObjects()
        {
            var detections = GetDetections(@"from t in tblA select t.col1, t.m_meas1");
            
            detections.TableReferences.Should().ContainSingle().Subject.TableName.Should().Be("tblA");

            var colRef = detections.ColumnReferences.Should().ContainSingle().Subject;
            colRef.TableName.Should().Be("tblA");
            colRef.Column.Should().Be("col1");

            var measRef = detections.MeasureReferences.Should().ContainSingle().Subject;
            measRef.TableName.Should().Be("tblA");
            measRef.Measure.Should().Be("m_meas1");
        }

        [Fact]
        public void DetectObjectsIncludingSubquery()
        {
            string input =
@"from subquery in {
        from a in [Acuity Tier (Previous Service)],
            c in [Care Changes]
        orderby c.[Service Flows for graphing] descending
        select a.[Simplified Service Type]
        top 5 },
    a in [Acuity Tier (T-1)]
where a.[Service Type] in subquery";

            var detections = new Detections();
            var query = parser.ParseFilter(input);
            query.DetectReferences(detections);

            VerifyDetections(detections,
                tableReferences: new Dictionary<string, int> { 
                    { "Acuity Tier (Previous Service)", 1 },
                    { "Care Changes", 1 },
                    { "Acuity Tier (T-1)", 1 },
                },
                columnReferences: new Dictionary<string, int> { 
                    { "Care Changes.Service Flows for graphing", 1 },
                    { "Acuity Tier (T-1).Service Type", 1 },
                    { "Acuity Tier (Previous Service).Simplified Service Type", 1 }
                },
                measureReferences: new Dictionary<string, int> {}
            );

        }

        private void VerifyDetections(Detections actualDetections, Dictionary<string, int> tableReferences, Dictionary<string, int> columnReferences, Dictionary<string, int> measureReferences)
        {
            var tablesToFind = tableReferences.Keys.ToHashSet();
            foreach (var g in actualDetections.TableReferences.GroupBy(x => x.TableName))
            {
                if (tableReferences.TryGetValue(g.Key, out var refs))
                {
                    if(g.Count() != refs)
                        throw new Exception($"Detected table {g.Key} {g.Count()} times, but was expecting to find it {refs} times.");
                    tablesToFind.Remove(g.Key);
                }
                else
                    throw new Exception($"Detected table {g.Key} which was not among the expected tables.");
            }
            if (tablesToFind.Any())
                throw new Exception("The following tables were expected but were not found: " + String.Join(", ", tablesToFind));

            var columnsToFind = columnReferences.Keys.ToHashSet();
            foreach (var g in actualDetections.ColumnReferences.GroupBy(x => new { x.TableName, x.Column }))
            {
                string columnFullName = $"{g.Key.TableName}.{g.Key.Column}";
                if (columnReferences.TryGetValue(columnFullName, out var refs))
                {
                    if (g.Count() != refs)
                        throw new Exception($"Detected column {columnFullName} {g.Count()} times, but was expecting to find it {refs} times.");
                    columnsToFind.Remove(columnFullName);
                }
                else
                    throw new Exception($"Detected column {columnFullName} which was not among the expected tables.");
            }
            if (columnsToFind.Any())
                throw new Exception("The following columns were expected but were not found: " + String.Join(", ", columnsToFind));

            var measuresToFind = measureReferences.Keys.ToHashSet();
            foreach (var g in actualDetections.MeasureReferences.GroupBy(x => new { x.TableName, x.Measure }))
            {
                string measureFullName = $"{g.Key.TableName}.{g.Key.Measure}";
                if (measureReferences.TryGetValue(measureFullName, out var refs))
                {
                    if (g.Count() != refs)
                        throw new Exception($"Detected measure {measureFullName} {g.Count()} times, but was expecting to find it {refs} times.");
                    measuresToFind.Remove(measureFullName);
                }
                else
                    throw new Exception($"Detected measure {measureFullName} which was not among the expected tables.");
            }
            if (measuresToFind.Any())
                throw new Exception("The following measures were expected but were not found: " + String.Join(", ", measuresToFind));
        }

        private Detections GetDetections(string queryStr)
        {
            var detections = new Detections();
            var query = parser.ParseQuery(queryStr);
            query.DetectReferences(detections);
            return detections;
        }
    }
}
