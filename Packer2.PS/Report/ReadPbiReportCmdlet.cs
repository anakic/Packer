using DataModelLoader.Report;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommunications.Read, "PbiReport")]
    [OutputType(typeof(PowerBIReport))]
    public class ReadPbiReportCmdlet : StoreCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The report source. Can be a pbit file or a folder containing the unpacked report.")]
        [Alias("s")]
        public string Source { get; set; }

        protected override void ProcessRecord()
        {
            var reportStore = GetReportStore(Source, true);
            var reportModel = reportStore.Read();
            WriteObject(reportModel);
        }
    }
}