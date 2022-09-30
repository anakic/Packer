using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsLifecycle.Register, "TabularModelDataSources")]
    [OutputType(typeof(Database))]
    public class RegisterTabularModelDataSourceCmdlet : Cmdlet
    {
        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Model { get; set; }

        protected override void ProcessRecord()
        {
            IDataModelTransform transform = new RegisterDataSourcesTransform();
            WriteObject(transform.Transform(Model));
        }
    }
}
