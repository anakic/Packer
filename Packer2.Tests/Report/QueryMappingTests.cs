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

        [Fact]
        public void MoveColumnToExistingFromSourceDropOldSource()
        {
            var originalQueryStr = @"from t in tblA, t1 in tblB select t.col1, t1.col2";
            var expectedQueryStr = @"from t in tblB select t.col1, t.col2";

            var mappings = new Mappings();
            mappings.Table("tblA").MapObjectTo("col1", "col1", "tblB");

            PerformRenameAndVerifyQuery(originalQueryStr, mappings, expectedQueryStr);
        }

        [Fact]
        public void MoveInSubquery()
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

            string expected = @"from subquery in {
        from c in [Care Changes], a in [AT (T-1)]
        orderby c.[Service Flows for graphing] descending
        select a.[Simplified Service Type]
        top 5 },
    a in [Acuity Tier (T-1)]
where a.[Service Type] in subquery";

            var mappings = new Mappings();
            mappings.Table("Acuity Tier (Previous Service)").MapTo("AT (T-1)");

            var parser = new QueryParser(new TestDbInfoGetter());
            var query = parser.ParseFilter(input);
            query.MapReferences(mappings);
            var res = NormalizeNewlines(query.ToString());

            res.Should().Be(NormalizeNewlines(expected));
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
