using FluentAssertions;
using Microsoft.AnalysisServices.Tabular;
using Packer2.FileSystem;
using Packer2.Library.DataModel;
using Xunit;

namespace Packer2.Tests.DataModel
{
    public class FolderModelStoreTests
    {
        [Fact]
        public void Read()
        {
            var fileSystem = new MemoryFileSystem();
            fileSystem.Save("database.json", @"{
  ""name"": ""TestDB"",
  ""compatibilityLevel"": 1500,
  ""model"": {
    ""tables"": [
      {
        ""fileRef"": ""Tables\\table1\\table.json"",
        ""mappingZone"": ""Packer2.Library.DataModel.FolderDatabaseStore+TablesMapping""
      }
    ]
  }
}");

            fileSystem.Save("Tables\\table1\\table.json", @"{
  ""name"": ""table1"",
  ""columns"": [
    {
      ""name"": ""DataColumn1"",
      ""dataType"": ""string""
    },
    {
      ""type"": ""calculated"",
      ""name"": ""CalculatedColumn1"",
      ""dataType"": ""string"",
      ""fileRef-expression"": ""Columns\\CalculatedColumn1.dax""
    }
  ],
  ""partitions"": [
    {
      ""name"": ""An M partition"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\An M partition.m""
      }
    },
    {
      ""name"": ""An M partition with * funky<% name"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\An M partition with %42; funky%60;%37; name.m""
      }
    }
  ]
}");

            fileSystem.Save("Tables\\table1\\columns\\CalculatedColumn1.dax", @"1+2");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition.m", @"let source = #""myDataSource"" in source");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m", @"let source = #""otherDataSource"" in source");


            var store = new FolderDatabaseStore(fileSystem);
            var database = store.Read();

            // todo: assert properties
            database.Name.Should().Be("TestDB");
            database.CompatibilityLevel.Should().Be(1500);
            database.Model.Tables.Single().Name.Should().Be("table1");
            database.Model.Tables.Single().Columns.ElementAt(0).Name.Should().Be("DataColumn1");
            database.Model.Tables.Single().Columns.ElementAt(0).DataType.Should().Be(DataType.String);
            database.Model.Tables.Single().Columns.ElementAt(1).Name.Should().Be("CalculatedColumn1");
            database.Model.Tables.Single().Columns.ElementAt(1).DataType.Should().Be(DataType.String);
            (database.Model.Tables.Single().Columns.ElementAt(1) as CalculatedColumn)!.Expression.Should().Be("1+2");
            database.Model.Tables.Single().Partitions.ElementAt(0).Name = "An M partition";
            (database.Model.Tables.Single().Partitions.ElementAt(0).Source as MPartitionSource)!.Expression = "let source = #\"myDataSource\" in source";
            database.Model.Tables.Single().Partitions.ElementAt(1).Name = "An M partition with * funky<% name";
            (database.Model.Tables.Single().Partitions.ElementAt(1).Source as MPartitionSource)!.Expression = "let source = #\"otherDataSource\" in source";
        }

        [Fact]
        public void Write()
        { 
            var fileSystem = new MemoryFileSystem();
            var store = new FolderDatabaseStore(fileSystem);

            var database = new Database("TestDB") { CompatibilityLevel = 1500, Model = new Model() };
            var table1 = new Table() { Name = "table1" };
            table1.Columns.Add(new DataColumn() { Name = "DataColumn1", DataType = DataType.String });
            table1.Columns.Add(new CalculatedColumn() { Name = "CalculatedColumn1", DataType = DataType.String, Expression = "1+2" });
            table1.Partitions.Add(new Partition() { Name = "An M partition", Source = new MPartitionSource() { Expression = "let source = #\"myDataSource\" in source" } });
            table1.Partitions.Add(new Partition() { Name = "An M partition with * funky<% name", Source = new MPartitionSource() { Expression = "let source = #\"otherDataSource\" in source" } });
            database.Model.Tables.Add(table1);

            store.Save(database);

            // todo: all of these asserts should be different tests
            fileSystem.GetFilesRecursive("").Count().Should().Be(5);
            fileSystem.ReadAsString("database.json").Should().Be(@"{
  ""name"": ""TestDB"",
  ""compatibilityLevel"": 1500,
  ""model"": {
    ""tables"": [
      {
        ""fileRef"": ""Tables\\table1\\table.json"",
        ""mappingZone"": ""Packer2.Library.DataModel.FolderDatabaseStore+TablesMapping""
      }
    ]
  }
}");
            fileSystem.ReadAsString("Tables\\table1\\table.json").Should().Be(@"{
  ""name"": ""table1"",
  ""columns"": [
    {
      ""name"": ""DataColumn1"",
      ""dataType"": ""string""
    },
    {
      ""type"": ""calculated"",
      ""name"": ""CalculatedColumn1"",
      ""dataType"": ""string"",
      ""fileRef-expression"": ""Columns\\CalculatedColumn1.dax""
    }
  ],
  ""partitions"": [
    {
      ""name"": ""An M partition"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\An M partition.m""
      }
    },
    {
      ""name"": ""An M partition with * funky<% name"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\An M partition with %42; funky%60;%37; name.m""
      }
    }
  ]
}");
            fileSystem.ReadAsString("Tables\\table1\\columns\\CalculatedColumn1.dax").Should().Be(@"1+2");
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition.m").Should().Be(@"let source = #""myDataSource"" in source");
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m").Should().Be(@"let source = #""otherDataSource"" in source");
        }

        //[Fact]
        //public void TEST_Dax()
        //{
        //    var file = new MemoryFile(TestResourcesHelper.GetOneTableTestModelContents());
        //    var store2 = new BimDataModelStore(file);
        //    var database = store2.Read();

        //    var storeSSAS = new SSASDataModelStore(".", "test_model_single_table", true);
        //    storeSSAS.Save(database);
        //}

    }
}
