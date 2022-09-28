using DataModelLoader.Report;
using Packer2.Library;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommon.Open, "PbiReport")]
    [OutputType(typeof(PowerBIReport))]
    public class OpenPbiReportCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = false, Position = 0)]
        [Alias("s")]
        public string Source { get; set; }

        protected override void ProcessRecord()
        {
            var path = Path.Combine(SessionState.Path.CurrentLocation.Path, Source);

            IModelStore<PowerBIReport> reportStore;
            if (File.Exists(path))
                reportStore = new PBIArchiveStore(path);
            else if (Directory.Exists(path))
                reportStore = new ReportFolderStore(path);
            else
                throw new Exception($"Source '{Source}' does not refer to an existing power bi file nor an unpacked folder");

            WriteObject(reportStore.Read());
        }
    }
}