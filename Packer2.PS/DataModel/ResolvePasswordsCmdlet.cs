using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet("Resolve", "Passwords")]
    [OutputType(typeof(Database))]
    public class ResolvePasswordsCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Database Database { get; set; }

        protected override void ProcessRecord()
        {
            var db = new PasswordResolveTransform().Transform(Database);
            WriteObject(db);
        }
    }
}
