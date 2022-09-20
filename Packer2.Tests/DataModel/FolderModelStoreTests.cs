using FluentAssertions;
using Packer2.Library;
using Packer2.Library.DataModel;
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
            var store = new FolderModelStore(testRepoFolder);
            var database = store.Read();
            database.Model.Tables.Should().NotBeEmpty();
        }

        [Fact]
        public void Write()
        {
            var store = new FolderModelStore(tempFolder);

            var file = new MemoryFile(TestResourcesHelper.GetTestModelContents());
            var store2 = new BimDataModelStore(file);
            var database = store2.Read();

            store.Save(database);
        }
    }
}
