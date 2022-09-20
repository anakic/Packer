using DataModelLoader.Report;
using Packer2.Library;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommon.Push, "PbiReport")]
    public class WritePbiReportCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = false, Position = 0)]
        [Alias("d")]
        public string Destionation { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PowerBIReport Report { get; set; }

        protected override void ProcessRecord()
        {
            IModelStore<PowerBIReport> reportStore;
            if (Path.HasExtension(Destionation))
                reportStore = new PBIArchiveStore(Destionation);
            else
                reportStore = new ReportFolderStore(Destionation);

            reportStore.Save(Report);
        }
    }
}