using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommunications.Read, "TabularModel")]
    [OutputType(typeof(Database))]
    public class ReadDataModelCmdlet : StoreCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("s")]
        public string Source { get; set; }

        [Parameter(Mandatory = false, Position = 1)]
        [Alias("c")]
        public string Customization { get; set; }

        protected override void ProcessRecord()
        {
            IModelStore<Database> store = GetDataModelStore(Source, Customization);
            WriteObject(store.Read());
        }
    }
}
