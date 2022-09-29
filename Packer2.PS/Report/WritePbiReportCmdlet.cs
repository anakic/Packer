using DataModelLoader.Report;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommunications.Read, "PbiReport")]
    public class WritePbiReportCmdlet : StoreCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
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