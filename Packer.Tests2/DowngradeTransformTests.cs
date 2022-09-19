using DataModelLoader;
using DataModelLoader.Transofrmations;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Packer.Tests2
{
    public class DowngradeTransformTests
    {
        [Fact]
        public void Downgrades()
        {
            var file = new InMemoryFile(File.ReadAllText(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\Model.bim"));
            var store = new BimDataModelStore(file);
            var database = store.Read();

            List<IDataModelTransform> transforms = new List<IDataModelTransform>()
            {
                new DowngradeTransform(1500),
                new PullUpExpressionsTranform(),
                new DeclareDataSourcesTransform(),
                new StripLocalDateTablesTransform(),
                new StripCulturesTransform(),
            };
            transforms.ForEach(t => t.Transform(database));

            store.Save(database);
            var x = file.Text;
        }
    }
}
