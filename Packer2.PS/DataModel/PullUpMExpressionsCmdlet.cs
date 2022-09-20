using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet("Transform", "PullUpMExpressions")]
    [OutputType(typeof(Database))]
    public class PullUpMExpressionsCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Model { get; set; }

        protected override void ProcessRecord()
        {
            IDataModelTransform transform = new PullUpExpressionsTranform();
            WriteObject(transform.Transform(Model));
        }
    }
}
