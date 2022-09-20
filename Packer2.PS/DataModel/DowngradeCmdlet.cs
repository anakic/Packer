using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet("Transform", "Downgrade")]
    [OutputType(typeof(Database))]
    public class DowngradeCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("ver", "v")]
        public int Version { get; set; }

        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Model { get; set; }

        protected override void ProcessRecord()
        {
            IDataModelTransform transform = new DowngradeTransform(Version);
            WriteObject(transform.Transform(Model));
        }
    }
}
