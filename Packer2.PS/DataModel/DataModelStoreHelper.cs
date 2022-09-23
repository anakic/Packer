using DataModelLoader.Report;
using Packer2.Library;
using Packer2.Library.DataModel;
using System.Data.SqlClient;

namespace Packer2.PS.DataModel
{
    static class StoreHelper
    {
        public static IModelStore<PowerBIReport> GetReportStore(string currentPath, string? path)
        {
            string combinedPath = currentPath;
            if (!string.IsNullOrEmpty(path))
                combinedPath = Path.Combine(currentPath, path); //if path is already rooted it will ignore the first arg

            if (Path.HasExtension(combinedPath))
                return new PBIArchiveStore(combinedPath);
            else
                return new ReportFolderStore(combinedPath);
        }

        public static IDataModelStore GetDataModelStore(string currentPath, string? location)
        {
            string combinedPath = currentPath;
            if (!string.IsNullOrEmpty(location))
                combinedPath = Path.Combine(currentPath, location); //if path is already rooted it will ignore the first arg

            IDataModelStore store;
            if (Path.IsPathRooted(combinedPath))
            {
                if (Path.HasExtension(combinedPath))
                    store = new BimDataModelStore(new LocalTextFile(combinedPath));
                else
                    store = new FolderModelStore(combinedPath);
            }
            else
            {
                var connStrBuilder = new SqlConnectionStringBuilder(location);
                store = new SSASDataModelStore(connStrBuilder.DataSource, connStrBuilder.InitialCatalog);
            }

            return store;
        }
    }
}
