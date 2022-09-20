using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet("Transform", "StripAutoTimeIntelligence")]
    [OutputType(typeof(Database))]
    public class StripAutoTimeIntelligenceCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Model { get; set; }

        protected override void ProcessRecord()
        {
            IDataModelTransform transform = new StripLocalDateTablesTransform();
            WriteObject(transform.Transform(Model));
        }
    }
}
