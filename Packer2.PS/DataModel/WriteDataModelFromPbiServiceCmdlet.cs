using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using Packer2.Library.DataModel;
using System.Management.Automation;
using System.Security;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommunications.Write, "TabularModelToPbiService")]
    [OutputType(typeof(Database))]
    public class WriteDataModelFromPbiServiceCmdlet : StoreCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("x")]
        public string XmlaEndpoint { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        [Alias("d")]
        public string DatabaseName { get; set; }

        [Parameter(Mandatory = true, Position = 2)]
        [Alias("wid")]
        public Guid WorkspaceId { get; set; }

        [Parameter(Mandatory = true, Position = 3)]
        [Alias("tid")]
        public string TenantId { get; set; }

        [Parameter(Mandatory = true, Position = 4)]
        [Alias("aid")]
        public string AppId { get; set; }

        [Parameter(Mandatory = true, Position = 5)]
        [Alias("pwd")]
        public string AppSecret { get; set; }

        [Parameter(Mandatory = false, Position = 6)]
        [Alias("gw")]
        public string GatewayName { get; set; }

        [Parameter(Mandatory = false, Position = 7)]
        [Alias("proc")]
        public AutoProcessBehavior AutoProcessBehavior { get; set; } = AutoProcessBehavior.Sequential;

        [Parameter(Mandatory = false, Position = 8)]
        [Alias("dsun")]
        public string? DataSourceUsername { get; set; }

        [Parameter(Mandatory = false, Position = 8)]
        [Alias("dspwd")]
        public SecureString? DataSourcePassword { get; set; }

        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Database { get; set; }

        protected override void ProcessRecord()
        {
            IModelStore<Database> store = new PowerBIServiceDataModelStore(
                XmlaEndpoint,
                DatabaseName,
                WorkspaceId,
                TenantId,
                AppId,
                AppSecret,
                AutoProcessBehavior,
                CreateLogger<PowerBIServiceDataModelStore>(),
                DataSourceUsername,
                DataSourcePassword
                )
            {
                GatewayName = GatewayName
            };

            store.Save(Database);
        }
    }
}
