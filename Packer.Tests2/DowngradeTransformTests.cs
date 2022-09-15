using DataModelLoader;
using DataModelLoader.Transofrmations;
using Xunit;

namespace Packer.Tests2
{
    public class DowngradeTransformTests
    {
        [Fact]
        public void Downgrades()
        {
            var file = new InMemoryFile(TestResourcesHelper.GetTestModelContents());
            var store = new BimDataModelStore(file);
            var database = store.Read();

            var transform = new PullUpExpressions();
            transform.Transform(database);

            store.Save(database);

            var x = file.Text;
        }
    }
}
