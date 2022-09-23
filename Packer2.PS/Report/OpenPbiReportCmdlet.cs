using DataModelLoader.Report;
using Packer2.Library;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommon.Open, "PbiReport")]
    [OutputType(typeof(PowerBIReport))]
    public class OpenPbiReportCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = false, Position = 0)]
        [Alias("s")]
        public string Source { get; set; }

        protected override void ProcessRecord()
        {
            IModelStore<PowerBIReport> reportStore;
            if (File.Exists(Source))
                reportStore = new PBIArchiveStore(Source);
            else if (Directory.Exists(Source))
                reportStore = new ReportFolderStore(Source);
            else
                throw new Exception("Source '{}' does not refer to an existing power bi file nor an unpacked folder");

            WriteObject(reportStore.Read());
        }
    }
}