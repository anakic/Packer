using DataModelLoader.Report;
using Packer2.Library;
using Packer2.PS.DataModel;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommon.Push, "PbiReport")]
    public class WritePbiReportCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = false, Position = 0)]
        [Alias("d")]
        public string Destionation { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PowerBIReport Report { get; set; }

        protected override void ProcessRecord()
        {
            var store = StoreHelper.GetReportStore(SessionState.Path.CurrentLocation.Path, Destionation);
            store.Save(Report);
        }
    }
}