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
    public class SSASDataModelStoreTest
    {
        [Fact]
        public void ReadsDataModel()
        {
            var store = new SSASDataModelStore("localhost:54287", null);
            var database = store.Read();
            database.Model.Tables.Count.Should().NotBe(0);

            var store2 = new BimDataModelStore(new TextBasedFile(@"C:\Projects\Packer\Packer.Tests2\TestFiles\test_model.bim"));
            store2.Save(database);
        }
    }
}
