using DataModelLoader.Report;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommon.Switch, "PbiReportToLocalDataModel")]
    [OutputType(typeof(PowerBIReport))]
    public class SwitchPbiReportToLocalDataModel : StoreCmdletBase
    {
        [Parameter(ValueFromPipeline = true)]
        public PowerBIReport Report { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(Report.SwitchToLocalDataModel());
        }
    }
}