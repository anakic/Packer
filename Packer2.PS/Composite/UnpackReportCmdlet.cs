using DataModelLoader.Report;
using Packer2.Library;
using Packer2.Library.DataModel;
using System.Management.Automation;

namespace Packer2.PS.Composite
{
    [Cmdlet("Unpack", "Report")]
    public class UnpackReportCmdlet : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [Alias("s")]
        public string Source { get; set; }

        [Parameter(Position = 1, Mandatory = false)]
        [Alias("d")]
        public string Destination { get; set; }

        [Parameter(Mandatory = false)]
        [Alias("m")]
        public bool UnpackDataModel { get; set; } = true;

        protected override void ProcessRecord()
        {
            var destinationFolder = Destination ?? SessionState.Path.CurrentFileSystemLocation.Path;

            var archiveStore = new PBIArchiveStore(Source);
            var reportModel = archiveStore.Read();

            var dataModelSchema = reportModel.DataModelSchemaFile;

            if (UnpackDataModel)
            {
                // replace the original datamodelschema file with the data model folder
                reportModel.DataModelSchemaFile = null;
            }

            var folderStore = new ReportFolderStore(destinationFolder);
            folderStore.Save(reportModel);

            // must let the report folderStore complete first because it clears the repo folder first before saving
            // then we can unpack the data model files
            if (dataModelSchema != null)
            {
                var bimModelStore = new BimDataModelStore(new MemoryFile(dataModelSchema.ToString()));
                var database = bimModelStore.Read();
                var folderModelStore = new FolderModelStore(Path.Combine(destinationFolder, "Data Model Schema"/*todo: define const and share with pack-report cmdlet*/));
                folderModelStore.Save(database);
            }

            base.ProcessRecord();
        }
    }
}
