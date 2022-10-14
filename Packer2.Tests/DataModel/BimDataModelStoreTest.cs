using FluentAssertions;
using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Tests.Tools;
using Xunit;

namespace Packer2.Tests.DataModel
{
    public class BimDataModelStoreTest
    {
        [Fact]
        public void ReadsDataModel()
        {
            var databaseStr = @"{
  ""name"": ""Test"",
  ""compatibilityLevel"": 1400,
  ""model"": {
    ""tables"": [
      {
        ""name"": ""My table"",
        ""columns"": [
          {
            ""name"": ""SomeNumber"",
            ""dataType"": ""double""
          }
        ]
      }
    ]
  }
}";

            var file = new MemoryFile(databaseStr);
            var store = new BimDataModelStore(file);
            var database = store.Read();
            database.Name.Should().Be("Test");
            database.CompatibilityLevel.Should().Be(1400);
            database.Model.Tables.Single().Name.Should().Be("My table");
            database.Model.Tables.Single().Columns.Single().Name.Should().Be("SomeNumber");
            database.Model.Tables.Single().Columns.Single().DataType.Should().Be(DataType.Double);
        }

        [Fact]
        public void SavesDataModel()
        {
            var database = new Database("Test")
            {
                CompatibilityLevel = 1400,
                Model = new Model()
            };

            var table1 = new Table() { Name = "My table" };
            table1.Columns.Add(new DataColumn() { Name = "SomeNumber", DataType = DataType.Double });
            database.Model.Tables.Add(table1);

            var file2 = new MemoryFile();
            var store2 = new BimDataModelStore(file2);
            store2.Save(database);

            file2.Text.Should().Be(
@"{
  ""name"": ""Test"",
  ""compatibilityLevel"": 1400,
  ""model"": {
    ""tables"": [
      {
        ""name"": ""My table"",
        ""columns"": [
          {
            ""name"": ""SomeNumber"",
            ""dataType"": ""double""
          }
        ]
      }
    ]
  }
}");
        }
    }
}
