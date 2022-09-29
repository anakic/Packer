using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsData.Import, "TabularModelDataSources")]
    [OutputType(typeof(Database))]
    public class ImportTabularModelDataSourcesCmdlet : StoreCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Database Database { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        [Alias("s")]
        public string SourceModel { get; set; }

        protected override void ProcessRecord()
        {
            var sourceDb = GetDataModelStore(SourceModel).Read();
            new MergeDataSourcesTransform(sourceDb).Transform(Database);
            WriteObject(Database);
        }
    }
}
