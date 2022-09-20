using Packer2.Library;
using Packer2.Library.DataModel;
using System.Data.SqlClient;

namespace Packer2.PS.DataModel
{
    static class DataModelStoreHelper
    {
        public static IDataModelStore GetStore(string location)
        {
            IDataModelStore store;

            if (File.Exists(location))
                store = new BimDataModelStore(new LocalTextFile(location));
            else if (Directory.Exists(location))
                store = new FolderModelStore(location);
            else
            {
                var connStrBuilder = new SqlConnectionStringBuilder(location);
                store = new SSASDataModelStore(connStrBuilder.DataSource, connStrBuilder.InitialCatalog);
            }

            return store;
        }
    }
}
