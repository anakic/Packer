using DataModelLoader;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Packer.Tests2
{
    public class BimDataModelStoreTest
    {
        [Fact]
        public void ReadsDataModel()
        {
            var file = new InMemoryFile(TestResourcesHelper.GetTestModelContents());
            var store = new BimDataModelStore(file);
            var database = store.Read();
            database.Model.Tables.Should().NotBeEmpty();
            // todo: proper tests
        }


        [Fact]
        public void SavesDataModel()
        {
            var file = new InMemoryFile(TestResourcesHelper.GetTestModelContents());
            var store = new BimDataModelStore(file);
            var database = store.Read();

            var file2 = new InMemoryFile();
            var store2 = new BimDataModelStore(file2);
            store2.Save(database);

            file2.Text.Should().Be(file.Text);
        }
    }
}
