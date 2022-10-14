using FluentAssertions;
using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Transofrmations;
using Packer2.Tests.Tools;
using Xunit;

namespace Packer2.Tests.DataModel
{
// these are for ad hoc testing, not for unit testing. they can't be run by CI runner because it doesn't have access to an SSAS instance.
#if DEBUG
    public class SSASDataModelStoreTest
    {
        [Fact]
        public void ReadsDataModel()
        {
            var store = new SSASDataModelStore("localhost:54287", null, true);
            var database = store.Read();
            database.Model.Tables.Count.Should().NotBe(0);

            var store2 = new BimDataModelStore(new TextFileStore(@"C:\Projects\Packer\Packer.Tests2\TestFiles\test_model.bim"));
            store2.Save(database);
        }

        [Fact]
        public void SavesDataModel()
        {
            var simpleModelBimContents = TestResourcesHelper.GetOneTableTestModelContents();
            var file = new MemoryFile(simpleModelBimContents);
            var store = new BimDataModelStore(file);
            var model = store.Read();

            model.Model.DataSources.OfType<StructuredDataSource>().Single().Credential.Password = "Discover2020*";

            var store2 = new SSASDataModelStore("localhost", "MyModel_SingleTable_3", true);
            store2.Save(model);
        }


        [Fact]
        public void TEMP()
        {
            var simpleModelBimContents = TestResourcesHelper.GetOneTableTestModelContents();
            var file = new MemoryFile(simpleModelBimContents);
            var store = new BimDataModelStore(file);
            var model = store.Read();

            var store2 = new BimDataModelStore(new MemoryFile(TestResourcesHelper.GetDSOnlyModelContents()));
            new MergeDataSourcesTransform(store2.Read()).Transform(model);

            var ds = model.Model.DataSources["SQL/DataflowDB"].Should().BeOfType<StructuredDataSource>().Subject;
            ds.Credential.Username.Should().Be("TEST_user");
            ds.ConnectionDetails.Address.Server.Should().Be("TEST_SERVER");
        }
    }
#endif
}
