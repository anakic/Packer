using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommon.Open, "TabularModel")]
    [OutputType(typeof(Database))]
    public class OpenDataModelCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("s")]
        public string Source { get; set; }

        protected override void ProcessRecord()
        {
            IDataModelStore store = DataModelStoreHelper.GetStore(Source);
            WriteObject(store.Read());
        }
    }
}
