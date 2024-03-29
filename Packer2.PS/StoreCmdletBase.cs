﻿using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.Report.Stores.Folder;
using System.Management.Automation;

namespace Packer2.PS
{
    public abstract class StoreCmdletBase : PSCmdlet
    {
        ILogger<StoreCmdletBase> logger;
        public StoreCmdletBase()
        {
            logger = this.CreateLogger<StoreCmdletBase>();
        }

        protected IModelStore<PowerBIReport> GetReportStore(string? path, bool checkExists = false)
        {
            string currentPath = SessionState.Path.CurrentLocation.Path;
            string combinedPath = currentPath;
            if (!string.IsNullOrEmpty(path))
            {
                combinedPath = Path.Combine(currentPath, path); //if path is already rooted it will ignore the first arg
            }

            IModelStore<PowerBIReport> store;
            if (Path.HasExtension(combinedPath))
            {
                if (checkExists && !File.Exists(combinedPath))
                    throw new Exception($"Report file not found at path {combinedPath}");
                store = new PBIArchiveStore(combinedPath, CreateLogger<PBIArchiveStore>());
                logger = this.CreateLogger<StoreCmdletBase>();
                logger.LogTrace("Created .pbit/.pbix report model store '({location})'", combinedPath);
            }
            else
            {
                if(checkExists && !Directory.Exists(combinedPath))
                    throw new Exception($"Report folder not found at path {combinedPath}");
                store = new ReportFolderStore(combinedPath, CreateLogger<ReportFolderStore>());
                logger.LogTrace("Created folder report model store '({location})'", combinedPath);
            }
            return store;
        }

        protected ILogger<T> CreateLogger<T>()
            => new PSLogger<T>(this);

        protected virtual AutoProcessBehavior AutoProcessBehavior => AutoProcessBehavior.Default;

        protected IModelStore<Database> GetDataModelStore(string? location, string? customization = null)
        {
            string currentPath = SessionState.Path.CurrentLocation.Path;
            IModelStore<Database> store;
            try
            {
                store = new SSASDataModelStore(location, AutoProcessBehavior, CreateLogger<SSASDataModelStore>());
                logger.LogTrace("Created SSAS data model store '({location})'", location);
            }
            catch
            {
                string combinedPath = currentPath;
                if (!string.IsNullOrEmpty(location))
                    combinedPath = Path.GetFullPath(Path.Combine(currentPath, location)); //if path is already rooted it will ignore the first arg

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
                        logger.LogTrace("Created .pbit file data model store '({location})'", location);
                    }
                    else
                    {
                        store = new BimDataModelStore(new TextFileStore(combinedPath));
                        logger.LogTrace("Created .bim file data model store '({location})'", location);
                    }
                }
                else
                {
                    store = new FolderDatabaseStore(combinedPath, customization, CreateLogger<FolderDatabaseStore>());
                    logger.LogTrace("Created folder data model store '({location})'", location);
                }
            }

            return store;
        }
    }
}

