using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Packer2.Library;
using Packer2.Library.DataModel;
using System.Data.SqlClient;
using System.Management.Automation;

namespace Packer2.PS
{
    public abstract class StoreCmdletBase : PSCmdlet
    {
        protected IModelStore<PowerBIReport> GetReportStore(string? path, bool checkExists = false)
        {
            string currentPath = SessionState.Path.CurrentLocation.Path;
            string combinedPath = currentPath;
            if (!string.IsNullOrEmpty(path))
                combinedPath = Path.Combine(currentPath, path); //if path is already rooted it will ignore the first arg

            if (Path.HasExtension(combinedPath))
            {
                if (checkExists && !File.Exists(combinedPath))
                    throw new Exception($"Report file not found at path {combinedPath}");
                return new PBIArchiveStore(combinedPath);
            }
            else
            {
                if(checkExists && !Directory.Exists(combinedPath))
                    throw new Exception($"Report folder not found at path {combinedPath}");
                return new ReportFolderStore(combinedPath);
            }
        }

        protected ILogger<T> CreateLogger<T>()
            => new PSLogger<T>(this);

        protected IDataModelStore GetDataModelStore(string? location)
        {
            string currentPath = SessionState.Path.CurrentLocation.Path;
            IDataModelStore store;
            try
            {
                var connStrBuilder = new SqlConnectionStringBuilder(location);
                store = new SSASDataModelStore(connStrBuilder.DataSource, connStrBuilder.InitialCatalog, true, CreateLogger<SSASDataModelStore>());
            }
            catch
            {
                string combinedPath = currentPath;
                if (!string.IsNullOrEmpty(location))
                    combinedPath = Path.Combine(currentPath, location); //if path is already rooted it will ignore the first arg

                if (Path.HasExtension(combinedPath))
                {
                    var extension = Path.GetExtension(combinedPath);
                    if (extension.ToLower().TrimStart('.') == "pbit")
                    {
                        var reportStore = GetReportStore(combinedPath);
                        var reportModel = reportStore.Read();
                        if (reportModel.DataModelSchemaFile == null)
                            throw new ArgumentException($"Cannot read data model from pbit file at {combinedPath}. This report does not have an integrated data model.");
                        store = new BimDataModelStore(new MemoryFile(reportModel.DataModelSchemaFile.ToString()));
                    }
                    else
                        store = new BimDataModelStore(new LocalTextFile(combinedPath));
                }
                else
                {
                    store = new FolderModelStore(combinedPath);
                }
            }

            return store;
        }
    }
}

