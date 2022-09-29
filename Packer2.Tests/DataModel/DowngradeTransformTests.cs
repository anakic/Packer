using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Transofrmations;
using Xunit;

namespace Packer2.Tests.DataModel
{
    public class DowngradeTransformTests
    {
        [Fact]
        public void Downgrades()
        {
            var file = new MemoryFile(File.ReadAllText(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\Model.bim"));
            var store = new BimDataModelStore(file);
            var database = store.Read();

            List<IDataModelTransform> transforms = new List<IDataModelTransform>()
            {
                new DowngradeTransform(1500),
                new PullUpExpressionsTranform(),
                new ExportDataSourcesTransform(),
                new StripLocalDateTablesTransform(),
                new StripCulturesTransform(),
            };
            transforms.ForEach(t => t.Transform(database));

            store.Save(database);
            var x = file.Text;
        }
    }
}
