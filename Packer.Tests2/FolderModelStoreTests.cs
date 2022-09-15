using DataModelLoader;
using FluentAssertions;
using System.IO;
using Xunit;

namespace Packer.Tests2
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

            var file = new InMemoryFile(TestResourcesHelper.GetTestModelContents());
            var store2 = new BimDataModelStore(file);
            var database = store2.Read();

            store.Save(database);
        }
    }
}
