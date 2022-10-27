using DataModelLoader.Report;
using Packer2.Library.Report.Transforms;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommon.Switch, "PbiReportDataModelReference")]
    public class SwitchPbiReportDataModelReferenceCmdlet : StoreCmdletBase
    {
        [Parameter(ValueFromPipeline = true)]
        public PowerBIReport Report { get; set; }

        [Parameter(Mandatory = true)]
        [Alias("t")]
        public string TableName { get; set; }

        [Parameter(Mandatory = true)]
        [Alias("on")]
        public string OriginalName { get; set; }
        
        [Parameter(Mandatory = true)]
        [Alias("nn")]
        public string NewName { get; set; }

        protected override void ProcessRecord()
        {
            var renames = new Renames();
            renames.AddRename(TableName, OriginalName, NewName);
            var transform = new ReplaceModelReferenceTransform(renames, CreateLogger<ReplaceModelReferenceTransform>());
            transform.Transform(Report);

            base.ProcessRecord();

            WriteObject(Report);
        }
    }
}
