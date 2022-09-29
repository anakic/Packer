using DataModelLoader.Report;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommunications.Write, "PbiReport")]
    public class WritePbiReportCmdlet : StoreCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "The destination where the report will be written to. Can be a .pbit file, a .pbix file, or a folder.")]
        [Alias("d")]
        public string Destionation { get; set; }

        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public PowerBIReport Report { get; set; }

        protected override void ProcessRecord()
        {
            var store = GetReportStore(Destionation);
            store.Save(Report);
        }
    }
}