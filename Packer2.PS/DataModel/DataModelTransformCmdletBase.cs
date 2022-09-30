using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [OutputType(typeof(Database))]
    public abstract class DataModelTransformCmdletBase : StoreCmdletBase
    {
        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Database { get; set; }

        protected abstract IDataModelTransform CreateTransform();

        protected override void ProcessRecord()
        {
            IDataModelTransform transform = CreateTransform();
            var transformed = transform.Transform(Database);
            WriteObject(transform.Transform(transformed));
        }
    }
}
