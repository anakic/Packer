using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommunications.Read, "TabularModel")]
    [OutputType(typeof(Database))]
    public class RaedDataModelCmdlet : StoreCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("s")]
        public string Source { get; set; }

        protected override void ProcessRecord()
        {
            IModelStore<Database> store = GetDataModelStore(Source);
            WriteObject(store.Read());
        }
    }
}
