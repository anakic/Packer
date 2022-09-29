using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsData.Export, "TabularModelDataSources")]
    [OutputType(typeof(Database))]
    public class DeclareDataSourceCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Model { get; set; }

        protected override void ProcessRecord()
        {
            IDataModelTransform transform = new ExportDataSourcesTransform();
            WriteObject(transform.Transform(Model));
        }
    }
}
