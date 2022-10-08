using FluentAssertions;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Transofrmations;
using Packer2.Tests.Tools;
using Xunit;

namespace Packer2.Tests.DataModel
{
    public class FolderModelStoreTests
    {
        static readonly string testRepoFolder = @"C:\Projects\Packer\Packer.Tests2\TestFiles\UnpackedDb";
        static readonly string tempFolder = Path.Combine(Path.GetTempPath(), nameof(FolderModelStoreTests));

        [Fact]
        public void Read()
        {
            var store = new FolderDatabaseStore(testRepoFolder);
            var database = store.Read();
            database.Model.Tables.Should().NotBeEmpty();
        }

        [Fact]
        public void Write()
        { 
            var store = new FolderDatabaseStore(tempFolder);

            var file = new MemoryFile(TestResourcesHelper.GetTestModelContents());
            var store2 = new BimDataModelStore(file);
            var database = store2.Read();

            store.Save(database);

            var db2 = store.Read();

            var fileA = new MemoryFile();
            var storeA = new BimDataModelStore(fileA);
            storeA.Save(database);

            var fileB = new MemoryFile();
            var storeB = new BimDataModelStore(fileB);
            storeB.Save(db2);

            fileA.Text.Should().Be(fileB.Text);
        }

        [Fact]
        public void TEST_Dax()
        {
            var file = new MemoryFile(TestResourcesHelper.GetOneTableTestModelContents());
            var store2 = new BimDataModelStore(file);
            var database = store2.Read();

            var storeSSAS = new SSASDataModelStore(".", "test_model_single_table", true);
            storeSSAS.Save(database);
        }
    }
}
