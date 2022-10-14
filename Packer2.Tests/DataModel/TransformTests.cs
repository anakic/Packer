using FluentAssertions;
using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Transofrmations;
using System.Xml.Linq;
using Xunit;

namespace Packer2.Tests.DataModel
{
    public class TransformTests
    {
        [Fact]
        public void DowngradesCorrectly()
        {
            var db = new Database()
            {
                Name = "TestDb",
                CompatibilityLevel = 1550,
                Model = new Model()
            };

            db.Model.Tables.Add(new Table() { Name = "Table1", LineageTag = "value that's not valid for compatibility level 1500" });

            var expectedBimContents =
// no lineage tag, compatibilityLevel: 1500 
@"{
  ""name"": ""TestDb"",
  ""compatibilityLevel"": 1500,
  ""model"": {
    ""tables"": [
      {
        ""name"": ""Table1""
      }
    ]
  }
}";
            AssertTransformationEffect(db, new DowngradeTransform(1500), expectedBimContents);
        }

        [Fact]
        public void RegistersSQLDataSource_Syntax1()
        {
            var db = new Database()
            {
                Name = "TestDb",
                CompatibilityLevel = 1550,
                Model = new Model()
            };

            var table1 = new Table() { Name = "table1" };
            db.Model.Tables.Add(table1);
            table1.Partitions.Add(new Partition()
            {
                Source = new MPartitionSource()
                {
                    Expression = "let\n    Source = Sql.Database(\"server_name\", \"database_name\"),\n    dbth_WardStay = Source{[Schema=\"dbth\",Item=\"WardStay\"]}[Data]\nin\n    dbth_WardStay"
                }
            });

            // todo:
            // - should assert these two things explicitly instead of asserting this string
            // - add test for the other syntax where the server and db are referenced separately
            // - add test for csv/xls/xlsx files


            // data source explicitly declared, partition expression references data source instead of connecting directly
            AssertTransformationEffect(db, new RegisterDataSourcesTransform(), @"{
  ""name"": ""TestDb"",
  ""compatibilityLevel"": 1550,
  ""model"": {
    ""dataSources"": [
      {
        ""type"": ""structured"",
        ""name"": ""SQL/server_name;database_name"",
        ""connectionDetails"": {
          ""protocol"": ""tds"",
          ""address"": {
            ""server"": ""server_name"",
            ""database"": ""database_name""
          },
          ""authentication"": null,
          ""query"": null
        },
        ""credential"": {
          ""AuthenticationKind"": ""UsernamePassword"",
          ""Username"": ""..."",
          ""Password"": ""********""
        }
      }
    ],
    ""tables"": [
      {
        ""name"": ""table1"",
        ""partitions"": [
          {
            ""name"": ""Partition"",
            ""source"": {
              ""type"": ""m"",
              ""expression"": ""let\n    Source= #\""SQL/server_name;database_name\"",\n    dbth_WardStay = Source{[Schema=\""dbth\"",Item=\""WardStay\""]}[Data]\nin\n    dbth_WardStay""
            }
          }
        ]
      }
    ]
  }
}");

        }

        [Fact]
        public void RegistersSQLDataSource_EscapeDotForLocalhost()
        {
            var db = new Database()
            {
                Name = "TestDb",
                CompatibilityLevel = 1550,
                Model = new Model()
            };

            var table1 = new Table() { Name = "table1" };
            db.Model.Tables.Add(table1);
            table1.Partitions.Add(new Partition()
            {
                Source = new MPartitionSource()
                {
                    Expression = "let\n    Source = Sql.Database(\".\", \"database_name\"),\n    dbth_WardStay = Source{[Schema=\"dbth\",Item=\"WardStay\"]}[Data]\nin\n    dbth_WardStay"
                }
            });

            var transformedDb = new RegisterDataSourcesTransform().Transform(db);
            transformedDb.Model.DataSources.Single().Name.Should().Be("SQL/localhost;database_name");
        }

        [Fact]
        public void RegistersSQLDataSource_EscapeDotForIP()
        {
            var db = new Database()
            {
                Name = "TestDb",
                CompatibilityLevel = 1550,
                Model = new Model()
            };

            var table1 = new Table() { Name = "table1" };
            db.Model.Tables.Add(table1);
            table1.Partitions.Add(new Partition()
            {
                Source = new MPartitionSource()
                {
                    Expression = "let\n    Source = Sql.Database(\"12.23.34.45\", \"database_name\"),\n    dbth_WardStay = Source{[Schema=\"dbth\",Item=\"WardStay\"]}[Data]\nin\n    dbth_WardStay"
                }
            });

            var transformedDb = new RegisterDataSourcesTransform().Transform(db);
            transformedDb.Model.DataSources.Single().Name.Should().Be("SQL/12 23 34 45;database_name");
        }

        // todo: add tests for
        //    new PullUpExpressionsTranform(),
        //    new StripLocalDateTablesTransform(),
        //    new StripCulturesTransform(),

        private void AssertTransformationEffect(Database db, IDataModelTransform transform, string expectedBimContents)
        {
            var transformedDb = transform.Transform(db);
            var dbStr = JsonSerializer.SerializeDatabase(transformedDb);
            dbStr.Should().Be(expectedBimContents);
        }
    }
}
