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

        private Detections GetDetections(string queryStr)
        {
            var detections = new Detections();
            var query = parser.ParseQuery(queryStr);
            query.DetectReferences(detections);
            return detections;
        }
    }
}
