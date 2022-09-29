using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsData.Import, "DataSources")]
    [OutputType(typeof(Database))]
    public class MergeDataSourcesCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Database Database { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        [Alias("ds")]
        public string DataSourcesFilePath { get; set; }

        protected override void ProcessRecord()
        {
            new MergeDataSourcesTransform(new LocalTextFile(Path.Combine(SessionState.Path.CurrentLocation.Path, DataSourcesFilePath))).Transform(Database);
            WriteObject(Database);
        }
    }
}
