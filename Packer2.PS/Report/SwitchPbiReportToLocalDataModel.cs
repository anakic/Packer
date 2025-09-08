using DataModelLoader.Report;
using Packer2.Library.Report.Transforms;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommon.Switch, "PbiReportToLocalDataModel")]
    [OutputType(typeof(PowerBIReport))]
    public class SwitchPbiReportToLocalDataModel : StoreCmdletBase
    {
        [Parameter(Mandatory = false, ValueFromPipeline = false, Position = 0)]
        public string ConnectionString { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PowerBIReport Report { get; set; }

        protected override void ProcessRecord()
        {
            var transform = new SwitchDataSourceToLocalTransform(ConnectionString, CreateLogger<SwitchDataSourceToSSASTransform>());
            WriteObject(transform.Transform(Report));
        }
    }
}