using FluentAssertions;
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
            var file = new MemoryFile(TestResourcesHelper.GetTestModelContents());
            var store = new BimDataModelStore(file);
            var database = store.Read();
            database.Model.Tables.Should().NotBeEmpty();
            // todo: proper tests
        }

        [Fact]
        public void SavesDataModel()
        {
            var file = new MemoryFile(TestResourcesHelper.GetTestModelContents());
            var store = new BimDataModelStore(file);
            var database = store.Read();

            var file2 = new MemoryFile();
            var store2 = new BimDataModelStore(file2);
            store2.Save(database);

            file2.Text.Should().Be(file.Text);
        }
    }
}
