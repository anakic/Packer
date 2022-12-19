using FluentAssertions;
using Packer2.Library.Report.QueryTransforms.Antlr;
using Packer2.Library.Report.Transforms;
using Packer2.Tests.Tools;
using System.Text.RegularExpressions;
using Xunit;

namespace Packer2.Tests.Report
{
    public class QueryMappingTests
    {
        [Fact]
        public void RenameEntireTable()
        {
            var originalQueryStr = @"from t in tbl1 select t.col1";
            var expectedQueryStr = @"from t in tbl2 select t.col1";

            var mappings = new Mappings();
            mappings.Table("tbl1").MapTo("tbl2");

            PerformRenameAndVerifyQuery(originalQueryStr, mappings, expectedQueryStr);
        }

        [Fact]
        public void RenameColumn()
        {
            var originalQueryStr = @"from t in tbl1 select t.col1";
            var expectedQueryStr = @"from t in tbl1 select t.col2";

            var mappings = new Mappings();
            mappings.Table("tbl1").MapObjectTo("col1", "col2");

            PerformRenameAndVerifyQuery(originalQueryStr, mappings, expectedQueryStr);
        }

        [Fact]
        public void MoveColumnToNewFromSource()
        {
            var originalQueryStr = @"from t in tblA select t.col1";
            var expectedQueryStr = @"from t in tblB select t.col1";

            var mappings = new Mappings();
            mappings.Table("tblA").MapObjectTo("col1", "col1", "tblB");

            PerformRenameAndVerifyQuery(originalQueryStr, mappings, expectedQueryStr);
        }

        [Fact]
        public void MoveColumnToNewFromSourceKeepOldSource()
        {
            var originalQueryStr = @"from t in tblA select t.col1, t.col2";
            var expectedQueryStr = @"from t in tblA, t1 in tblB select t.col1, t1.col2";

            var mappings = new Mappings();
            mappings.Table("tblA").MapObjectTo("col2", "col2", "tblB");

            PerformRenameAndVerifyQuery(originalQueryStr, mappings, expectedQueryStr);
        }
        private void PerformRenameAndVerifyQuery(string originalQueryStr, Mappings mappings, string expectedQueryStr)
        {
            var parser = new QueryParser(new TestDbInfoGetter());
            var query = parser.ParseQuery(originalQueryStr);
            query.MapReferences(mappings);
            var res = NormalizeNewlines(query.ToString());
            res.Should().Be(NormalizeNewlines(expectedQueryStr));
        }

        Regex wsRx = new Regex(@"(\s)+", RegexOptions.Compiled);
        private string NormalizeNewlines(string input)
            => wsRx.Replace(input, " ");
    }
}
