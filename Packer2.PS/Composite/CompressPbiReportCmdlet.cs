using DataModelLoader.Report;
using Newtonsoft.Json.Linq;
using Packer2.Library;
using Packer2.Library.DataModel;
using System.Management.Automation;

namespace Packer2.PS.Composite
{
    [Cmdlet(VerbsData.Compress, "PbiReport")]
    public class CompressPbiReportCmdlet : StoreCmdletBase
    {
        [Parameter(Position = 0, Mandatory = true)]
        [Alias("s")]
        public string Source { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        [Alias("d")]
        public string Destination { get; set; }

        protected override void ProcessRecord()
        {
            var repoFolder = Path.Combine(SessionState.Path.CurrentLocation.Path, Source);
            var reportSourceStore = new ReportFolderStore(repoFolder, CreateLogger<ReportFolderStore>());
            var reportModel = reportSourceStore.Read();
            var dataModelFolder = Path.Combine(repoFolder, "Data Model Schema"/*todo: constant*/);
            if (Directory.Exists(dataModelFolder))
            {
                // load the DataModelSchema into the reportmodel from the folder (instead of from the DataModelSchema file)
                var folderModelStore = new FolderDatabaseStore(dataModelFolder);
                var database = folderModelStore.Read();
                var inMemoryFile = new MemoryFile();
                var bimModelStore = new BimDataModelStore(inMemoryFile);
                bimModelStore.Save(database);
                reportModel.DataModelSchemaFile = JObject.Parse(inMemoryFile.Text!);
            }
            var reportStore = GetReportStore(Destination);
            reportStore.Save(reportModel);
        }
    }
}
