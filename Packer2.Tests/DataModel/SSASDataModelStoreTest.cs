using DataModelLoader;
using FluentAssertions;
using Packer2.Library;
using Packer2.Library.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Packer2.Tests.DataModel
{
    public class SSASDataModelStoreTest
    {
        [Fact]
        public void ReadsDataModel()
        {
            var store = new SSASDataModelStore("localhost:54287", null);
            var database = store.Read();
            database.Model.Tables.Count.Should().NotBe(0);

            var store2 = new BimDataModelStore(new LocalTextFile(@"C:\Projects\Packer\Packer.Tests2\TestFiles\test_model.bim"));
            store2.Save(database);
        }
    }
}
