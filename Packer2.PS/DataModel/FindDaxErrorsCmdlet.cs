using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommon.Find, "DaxErrors")]
    [OutputType(typeof(Database))]
    public class FindDaxErrorsCmdlet:Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public Database Database { get; set; }

        protected override void ProcessRecord()
        {
            // todo: logger?
            var transform = new DetectDaxErrrorsTransform(err => WriteInformation(err, Array.Empty<string>()));
            transform.Transform(Database);
            WriteObject(Database);
        }
    }
}
