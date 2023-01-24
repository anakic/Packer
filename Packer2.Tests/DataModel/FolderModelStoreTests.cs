using FluentAssertions;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.InfoNav.Utils;
using Packer2.FileSystem;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Customizations;
using Xunit;

namespace Packer2.Tests.DataModel
{
    // Here be dragons: these tests are convoluted and tricky to set up, but I don't know if it's possible to simplify
    // or at least de-obfuscate the setup somehow. They involve both tweaking the contents of files and the tabular model
    // and it's also hard to separate the important settings in the scenario from the scaffolding. Consider what to do about this.

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


        [Fact]
        public void ReadCustomized()
        {
            // arrange
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
            fileSystem.Save($"{CustFileSystem.CustomizationsFolder}\\TEST_CUST\\Tables\\table1\\columns\\CalculatedColumn1.dax", @"1+2+3");

            // act
            var store = new FolderDatabaseStore(fileSystem, "TEST_CUST");
            var database = store.Read();

            // assert
            database.Name.Should().Be("TestDB");
            database.CompatibilityLevel.Should().Be(1500);
            database.Model.Tables.Single().Name.Should().Be("table1");
            database.Model.Tables.Single().Columns.ElementAt(0).Name.Should().Be("DataColumn1");
            database.Model.Tables.Single().Columns.ElementAt(0).DataType.Should().Be(DataType.String);
            database.Model.Tables.Single().Columns.ElementAt(1).Name.Should().Be("CalculatedColumn1");
            database.Model.Tables.Single().Columns.ElementAt(1).DataType.Should().Be(DataType.String);
            (database.Model.Tables.Single().Columns.ElementAt(1) as CalculatedColumn)!.Expression.Should().Be("1+2+3");
            database.Model.Tables.Single().Partitions.ElementAt(0).Name = "An M partition";
            (database.Model.Tables.Single().Partitions.ElementAt(0).Source as MPartitionSource)!.Expression = "let source = #\"myDataSource\" in source";
            database.Model.Tables.Single().Partitions.ElementAt(1).Name = "An M partition with * funky<% name";
            (database.Model.Tables.Single().Partitions.ElementAt(1).Source as MPartitionSource)!.Expression = "let source = #\"otherDataSource\" in source";
        }

        [Fact]
        public void WriteCustomized()
        {
            var fileSystem = new MemoryFileSystem();

            // Set initial stored data
            // - root and customized folder
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
            // - cust folder
            string customizationName = "TEST_CUST";
            string custFilePath = $"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\Tables\\table1\\columns\\CalculatedColumn1.dax";
            fileSystem.Save(custFilePath, @"1+2+3");


            // create model that corresponds to saved files with one change
            var database = new Database("TestDB") { CompatibilityLevel = 1500, Model = new Model() };
            var table1 = new Table() { Name = "table1" };
            table1.Columns.Add(new DataColumn() { Name = "DataColumn1", DataType = DataType.String });
            table1.Columns.Add(new CalculatedColumn() { Name = "CalculatedColumn1", DataType = DataType.String, Expression = "1+2+3+4" }); //<-- change is here (1+2+3+4 instead of 1+2+3, in the .cust folder)
            table1.Partitions.Add(new Partition() { Name = "An M partition", Source = new MPartitionSource() { Expression = "let source = #\"myDataSource\" in source" } });
            table1.Partitions.Add(new Partition() { Name = "An M partition with * funky<% name", Source = new MPartitionSource() { Expression = "let source = #\"otherDataSource\" in source" } });
            database.Model.Tables.Add(table1);

            // act
            var store = new FolderDatabaseStore(fileSystem, customizationName);
            store.Save(database);

            // assert
            fileSystem.GetFilesRecursive("").Count().Should().Be(6);
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
            // root file should be left alone
            fileSystem.ReadAsString("Tables\\table1\\columns\\CalculatedColumn1.dax").Should().Be(@"1+2");
            // cust file should be updated
            fileSystem.ReadAsString(custFilePath).Should().Be(@"1+2+3+4");
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition.m").Should().Be(@"let source = #""myDataSource"" in source");
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m").Should().Be(@"let source = #""otherDataSource"" in source");
        }

        [Fact]
        public void DeleteColumnCustomized()
        {
            var fileSystem = new MemoryFileSystem();

            // root
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
            // cust
            string customizationName = "TEST_CUST";
            string custFilePath = $"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\Tables\\table1\\columns\\CalculatedColumn1.dax";
            fileSystem.Save(custFilePath, @"1+2+3");

            // set up corresponding model but remove the calc column
            var database = new Database("TestDB") { CompatibilityLevel = 1500, Model = new Model() };
            var table1 = new Table() { Name = "table1" };
            table1.Columns.Add(new DataColumn() { Name = "DataColumn1", DataType = DataType.String });
            // do not add this column (in effect delete it from the store, it exists there)
            // table1.Columns.Add(new CalculatedColumn() { Name = "CalculatedColumn1", DataType = DataType.String, Expression = "1+2+3+4" });
            table1.Partitions.Add(new Partition() { Name = "An M partition", Source = new MPartitionSource() { Expression = "let source = #\"myDataSource\" in source" } });
            table1.Partitions.Add(new Partition() { Name = "An M partition with * funky<% name", Source = new MPartitionSource() { Expression = "let source = #\"otherDataSource\" in source" } });
            database.Model.Tables.Add(table1);

            // act
            var store = new FolderDatabaseStore(fileSystem, customizationName);
            store.Save(database);

            // assert
            fileSystem.GetFilesRecursive("").Count().Should().Be(4);
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
            // the calculated column should be deleted from both locations (cust and root)
            fileSystem.FileExists("Tables\\table1\\columns\\CalculatedColumn1.dax").Should().BeFalse();
            fileSystem.FileExists(custFilePath).Should().BeFalse();
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition.m").Should().Be(@"let source = #""myDataSource"" in source");
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m").Should().Be(@"let source = #""otherDataSource"" in source");
        }

        [Fact]
        public void DeleteColumn_OneColumnRemaining_Customized()
        {
            var fileSystem = new MemoryFileSystem();

            // root
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
    },
    {
      ""type"": ""calculated"",
      ""name"": ""CalculatedColumn2"",
      ""dataType"": ""string"",
      ""fileRef-expression"": ""Columns\\CalculatedColumn2.dax""
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
            fileSystem.Save("Tables\\table1\\columns\\CalculatedColumn2.dax", @"11+22");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition.m", @"let source = #""myDataSource"" in source");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m", @"let source = #""otherDataSource"" in source");
            // cust
            string customizationName = "TEST_CUST";
            string custFilePath = $"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\Tables\\table1\\columns\\CalculatedColumn1.dax";
            fileSystem.Save(custFilePath, @"1+2+3");


            // set up db with two calc columns (one customized), delete customized one, ensure the other one is left intact
            var database = new Database("TestDB") { CompatibilityLevel = 1500, Model = new Model() };
            var table1 = new Table() { Name = "table1" };
            table1.Columns.Add(new DataColumn() { Name = "DataColumn1", DataType = DataType.String });
            // do not add this column (in effect delete it from the store, it exists there)
            // table1.Columns.Add(new CalculatedColumn() { Name = "CalculatedColumn1", DataType = DataType.String, Expression = "1+2+3+4" });
            table1.Columns.Add(new CalculatedColumn() { Name = "CalculatedColumn2", DataType = DataType.String, Expression = "11+22" });
            table1.Partitions.Add(new Partition() { Name = "An M partition", Source = new MPartitionSource() { Expression = "let source = #\"myDataSource\" in source" } });
            table1.Partitions.Add(new Partition() { Name = "An M partition with * funky<% name", Source = new MPartitionSource() { Expression = "let source = #\"otherDataSource\" in source" } });
            database.Model.Tables.Add(table1);

            // act
            var store = new FolderDatabaseStore(fileSystem, customizationName);
            store.Save(database);

            // assert
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
      ""name"": ""CalculatedColumn2"",
      ""dataType"": ""string"",
      ""fileRef-expression"": ""Columns\\CalculatedColumn2.dax""
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
            // ensure non-deleted col is left intact
            fileSystem.ReadAsString("Tables\\table1\\columns\\CalculatedColumn2.dax").Should().Be(@"11+22");
            // ensude deleted col is removed from both locations
            fileSystem.FileExists("Tables\\table1\\columns\\CalculatedColumn1.dax").Should().BeFalse();
            fileSystem.FileExists(custFilePath).Should().BeFalse();
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition.m").Should().Be(@"let source = #""myDataSource"" in source");
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m").Should().Be(@"let source = #""otherDataSource"" in source");
        }

        [Fact]
        public void ReadWithIgnoreColumnCustomization()
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

            string customizationName = "My_TEST";
            fileSystem.Save($"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\packer.ignore", @"'table1'[DataColumn1]");

            var store = new FolderDatabaseStore(fileSystem, customizationName);
            var database = store.Read();

            // todo: assert properties
            database.Name.Should().Be("TestDB");
            database.CompatibilityLevel.Should().Be(1500);
            database.Model.Tables.Single().Name.Should().Be("table1");
            database.Model.Tables.Single().Columns.Single().Name.Should().Be("CalculatedColumn1");
            database.Model.Tables.Single().Columns.Single().DataType.Should().Be(DataType.String);
            (database.Model.Tables.Single().Columns.Single() as CalculatedColumn)!.Expression.Should().Be("1+2");
            database.Model.Tables.Single().Partitions.ElementAt(0).Name = "An M partition";
            (database.Model.Tables.Single().Partitions.ElementAt(0).Source as MPartitionSource)!.Expression = "let source = #\"myDataSource\" in source";
            database.Model.Tables.Single().Partitions.ElementAt(1).Name = "An M partition with * funky<% name";
            (database.Model.Tables.Single().Partitions.ElementAt(1).Source as MPartitionSource)!.Expression = "let source = #\"otherDataSource\" in source";
        }

        [Fact]
        public void WriteWithIgnoreColumnCustomization()
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

            string customizationName = "My_TEST";
            fileSystem.Save($"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\packer.ignore", @"'table1'[CalculatedColumn1]");



            var store = new FolderDatabaseStore(fileSystem, customizationName);
            var database = store.Read();

            database.Model.Tables["table1"].Columns.Contains("CalculatedColumn1").Should().BeFalse();
            store.Save(database);
            fileSystem.FileExists("Tables\\table1\\columns\\CalculatedColumn1.dax").Should().BeTrue();
        }

        [Fact]
        public void DeletingTableDeletesRelationshipCustomization()
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
      },
      {
        ""fileRef"": ""Tables\\related_table\\table.json"",
        ""mappingZone"": ""Packer2.Library.DataModel.FolderDatabaseStore+TablesMapping""
      }
    ],
    ""relationships"":[
            {
                ""name"": ""e357f34b-5961-499c-bc34-8fdec4d50db7"",
                ""fromTable"": ""table1"",
                ""fromColumn"": ""DataColumn1"",
                ""toTable"": ""related_table"",
                ""toColumn"": ""ColumnA""
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

            fileSystem.Save("Tables\\related_table\\table.json", @"{
  ""name"": ""related_table"",
  ""columns"": [
    {
      ""name"": ""ColumnA"",
      ""dataType"": ""string""
    },
    {
      ""type"": ""calculated"",
      ""name"": ""CalculatedColumn1"",
      ""dataType"": ""string"",
      ""fileRef-expression"": ""Columns\\CalculatedColumnA.dax""
    }
  ],
  ""partitions"": [
    {
      ""name"": ""An M partition"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\Another M partition.m""
      }
    },
    {
      ""name"": ""An M partition with * funky<% name"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\Another M partition with %42; funky%60;%37; name.m""
      }
    }
  ]
}");

            fileSystem.Save("Tables\\table1\\columns\\CalculatedColumn1.dax", @"1+2");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition.m", @"let source = #""myDataSource"" in source");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m", @"let source = #""otherDataSource"" in source");
            fileSystem.Save("Tables\\related_table\\columns\\CalculatedColumnA.dax", @"1+2");
            fileSystem.Save("Tables\\related_table\\partitions\\Another M partition.m", @"let source = #""myDataSource"" in source");
            fileSystem.Save("Tables\\related_table\\partitions\\Another M partition with %42; funky%60;%37; name.m", @"let source = #""otherDataSource"" in source");

            string customizationName = "My_TEST";
            fileSystem.Save($"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\packer.ignore", @"related_table");

            var store = new FolderDatabaseStore(fileSystem, customizationName);
            var database = store.Read();

            database.Model.Tables.Count.Should().Be(1);
            database.Model.Tables.Single().Name.Should().Be("table1");
            database.Model.Tables.Single().Columns.Count().Should().Be(2);
            database.Model.Relationships.Should().BeEmpty();
        }

        [Fact]
        public void DeletingColumnDeletesRelationshipCustomization()
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
      },
      {
        ""fileRef"": ""Tables\\related_table\\table.json"",
        ""mappingZone"": ""Packer2.Library.DataModel.FolderDatabaseStore+TablesMapping""
      }
    ],
    ""relationships"":[
            {
                ""name"": ""e357f34b-5961-499c-bc34-8fdec4d50db7"",
                ""fromTable"": ""table1"",
                ""fromColumn"": ""DataColumn1"",
                ""toTable"": ""related_table"",
                ""toColumn"": ""ColumnA""
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

            fileSystem.Save("Tables\\related_table\\table.json", @"{
  ""name"": ""related_table"",
  ""columns"": [
    {
      ""name"": ""ColumnA"",
      ""dataType"": ""string""
    },
    {
      ""type"": ""calculated"",
      ""name"": ""CalculatedColumn1"",
      ""dataType"": ""string"",
      ""fileRef-expression"": ""Columns\\CalculatedColumnA.dax""
    }
  ],
  ""partitions"": [
    {
      ""name"": ""An M partition"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\Another M partition.m""
      }
    },
    {
      ""name"": ""An M partition with * funky<% name"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\Another M partition with %42; funky%60;%37; name.m""
      }
    }
  ]
}");

            fileSystem.Save("Tables\\table1\\columns\\CalculatedColumn1.dax", @"1+2");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition.m", @"let source = #""myDataSource"" in source");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m", @"let source = #""otherDataSource"" in source");
            fileSystem.Save("Tables\\related_table\\columns\\CalculatedColumnA.dax", @"1+2");
            fileSystem.Save("Tables\\related_table\\partitions\\Another M partition.m", @"let source = #""myDataSource"" in source");
            fileSystem.Save("Tables\\related_table\\partitions\\Another M partition with %42; funky%60;%37; name.m", @"let source = #""otherDataSource"" in source");

            string customizationName = "My_TEST";
            fileSystem.Save($"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\packer.ignore", @"table1[DataColumn1]");

            var store = new FolderDatabaseStore(fileSystem, customizationName);
            var database = store.Read();

            database.Model.Tables.Count.Should().Be(2);
            database.Model.Tables["table1"].Columns.Count.Should().Be(1);
            database.Model.Tables["related_table"].Columns.Count.Should().Be(2);
            database.Model.Relationships.Should().BeEmpty();
        }

        // writing model that ignores a table keeps that table as well as all its relationships in the model
        [Fact]
        public void KeepsIgnoredTablesAndRelationshipsOnSave()
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
      },
      {
        ""fileRef"": ""Tables\\related_table\\table.json"",
        ""mappingZone"": ""Packer2.Library.DataModel.FolderDatabaseStore+TablesMapping""
      }
    ],
    ""relationships"":[
            {
                ""name"": ""e357f34b-5961-499c-bc34-8fdec4d50db7"",
                ""fromTable"": ""table1"",
                ""fromColumn"": ""DataColumn1"",
                ""toTable"": ""related_table"",
                ""toColumn"": ""ColumnA""
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
            fileSystem.Save("Tables\\related_table\\table.json", @"{
  ""name"": ""related_table"",
  ""columns"": [
    {
      ""name"": ""ColumnA"",
      ""dataType"": ""string""
    },
    {
      ""type"": ""calculated"",
      ""name"": ""CalculatedColumnA"",
      ""dataType"": ""string"",
      ""fileRef-expression"": ""Columns\\CalculatedColumnA.dax""
    }
  ],
  ""partitions"": [
    {
      ""name"": ""Another M partition"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\Another M partition.m""
      }
    },
    {
      ""name"": ""Another M partition with * funky<% name"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\Another M partition with %42; funky%60;%37; name.m""
      }
    }
  ]
}");
            fileSystem.Save("Tables\\table1\\columns\\CalculatedColumn1.dax", @"1+2");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition.m", @"let source = #""myDataSource"" in source");
            fileSystem.Save("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m", @"let source = #""otherDataSource"" in source");
            fileSystem.Save("Tables\\related_table\\columns\\CalculatedColumnA.dax", @"1+2");
            fileSystem.Save("Tables\\related_table\\partitions\\Another M partition.m", @"let source = #""myDataSource"" in source");
            fileSystem.Save("Tables\\related_table\\partitions\\Another M partition with %42; funky%60;%37; name.m", @"let source = #""otherDataSource"" in source");

            string customizationName = "My_TEST";
            fileSystem.Save($"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\packer.ignore", @"related_table");

            var store = new FolderDatabaseStore(fileSystem, customizationName);
            var database = store.Read();

            (database.Model.Tables["table1"].Columns["CalculatedColumn1"] as CalculatedColumn).Expression = "1+2+3";

            store.Save(database);

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
            fileSystem.Save("Tables\\related_table\\table.json", @"{
  ""name"": ""related_table"",
  ""columns"": [
    {
      ""name"": ""ColumnA"",
      ""dataType"": ""string""
    },
    {
      ""type"": ""calculated"",
      ""name"": ""CalculatedColumn1"",
      ""dataType"": ""string"",
      ""fileRef-expression"": ""Columns\\CalculatedColumnA.dax""
    }
  ],
  ""partitions"": [
    {
      ""name"": ""Another M partition"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\Another M partition.m""
      }
    },
    {
      ""name"": ""Another M partition with * funky<% name"",
      ""source"": {
        ""type"": ""m"",
        ""fileRef-expression"": ""Partitions\\Another M partition with %42; funky%60;%37; name.m""
      }
    }
  ]
}");
            fileSystem.ReadAsString("Tables\\table1\\columns\\CalculatedColumn1.dax").Should().Be(@"1+2+3");
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition.m").Should().Be(@"let source = #""myDataSource"" in source");
            fileSystem.ReadAsString("Tables\\table1\\partitions\\An M partition with %42; funky%60;%37; name.m").Should().Be(@"let source = #""otherDataSource"" in source");
            fileSystem.ReadAsString("Tables\\related_table\\columns\\CalculatedColumnA.dax").Should().Be(@"1+2");
            fileSystem.ReadAsString("Tables\\related_table\\partitions\\Another M partition.m").Should().Be(@"let source = #""myDataSource"" in source");
            fileSystem.ReadAsString("Tables\\related_table\\partitions\\Another M partition with %42; funky%60;%37; name.m").Should().Be(@"let source = #""otherDataSource"" in source");
            fileSystem.ReadAsString($"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\packer.ignore").Should().Be(@"related_table");
        }

        [Fact]
        public void HonoresGlobalIgnoreFile()
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

            string customizationName = "My_TEST";
            fileSystem.Save($"packer.ignore", @"*[CalculatedColumn1]");


            var store = new FolderDatabaseStore(fileSystem, customizationName);
            var database = store.Read();

            database.Model.Tables["table1"].Columns.Count.Should().Be(1);
            database.Model.Tables["table1"].Columns.Contains("CalculatedColumn1").Should().BeFalse();
            database.Model.Tables["table1"].Columns.Contains("DataColumn1").Should().BeTrue();
        }

        [Fact]
        public void CustIgnoreOverridesGlobalOne()
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

            string customizationName = "My_TEST";
            fileSystem.Save($"packer.ignore", @"*[CalculatedColumn1]");
            fileSystem.Save($"{CustFileSystem.CustomizationsFolder}\\{customizationName}\\packer.ignore", @"!*[CalculatedColumn1]");

            var store = new FolderDatabaseStore(fileSystem, customizationName);
            var database = store.Read();

            database.Model.Tables["table1"].Columns.Contains("CalculatedColumn1").Should().BeTrue();
            database.Model.Tables["table1"].Columns.Contains("DataColumn1").Should().BeTrue();
        }


        [Fact]
        public void WritingModelDoesNotDeleteGlobalIgnoreFile()
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

            string customizationName = "My_TEST";
            fileSystem.Save($"packer.ignore", @"*[CalculatedColumn1]");


            var store = new FolderDatabaseStore(fileSystem, customizationName);
            var database = store.Read();
            store.Save(database);

            fileSystem.FileExists("packer.ignore").Should().BeTrue();
        }
    }
}
