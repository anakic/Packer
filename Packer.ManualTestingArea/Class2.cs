using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api.Models;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using Packer2.Library;
using Packer2.Library.DataModel;
using Microsoft.Extensions.Logging;
using Microsoft.AnalysisServices.Tabular;

namespace Packer.ManualTestingArea
{
    class PowerBIServiceDataModelStore : SSASDataModelStore
    {
        private readonly Guid workspaceId;
        private readonly string tenantId;
        private readonly string appId;
        private readonly string appSecret;

        public PowerBIServiceDataModelStore(string workspaceConnection, string databaseName, Guid workspaceId, string tenantId, string appId, string appSecret, bool processOnSave = true, ILogger<PowerBIServiceDataModelStore>? logger = null) 
            : base($"Server={workspaceConnection};Database={databaseName};User ID=app:{appId}@{tenantId};Password={appSecret};", processOnSave, logger)
        {
            this.workspaceId = workspaceId;
            this.tenantId = tenantId;
            this.appId = appId;
            this.appSecret = appSecret;
        }

        protected override void OnDatabaseUpdated(Database database)
        {
            DoPbi(async client =>
            {
                var datasetId = database.ID;

                var ds = await client.Datasets.GetDatasourcesInGroupAsync(workspaceId, datasetId);

                // Get the datasource from your dataset
                var datasource = await client.Datasets.GetDatasourcesInGroupAsync(workspaceId, datasetId);
                var gw = (await client.Gateways.GetGatewaysAsync()).Value.Single();

                var ds2 = await client.Gateways.GetDatasourcesAsync(gw.Id);

                await client.Datasets.BindToGatewayInGroupAsync(workspaceId, datasetId, new BindToGatewayRequest(gw.Id));
            }).Wait();
        }

        async Task DoPbi(Func<PowerBIClient, Task> action)
        {
            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var scopes = new string[] { "https://analysis.windows.net/powerbi/api/.default" };

            var app = ConfidentialClientApplicationBuilder
            .Create(appId)
                .WithClientSecret(appSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            var tokenCredentials = new TokenCredentials(authResult.AccessToken, "Bearer");

            using (var client = new PowerBIClient(new Uri("https://api.powerbi.com"), tokenCredentials))
            {
                await action(client);
            }
        }
    }

    public class Class2
    {
        static Guid groupId = Guid.Parse("b060ead8-e4b4-4855-8ee1-633dddbd796c");
        static string appId = "efb3ae81-eb15-41de-ad47-01bb8370f5e8";
        static string tenantId = "9ac94aa1-d615-4174-8367-c2208fb31f6b";
        static string secret = "jTN8Q~-l73JaFx54AJqkWBnidTQP-6WYIxwWHaD-";

        public static void Main()
        {
            var db = TabularModel.LoadFromPbitFile(@"C:\Models\a\adwks_test.pbit");
            db.DeclareDataSources();

            var store23 = new PowerBIServiceDataModelStore($"powerbi://api.powerbi.com/v1.0/myorg/DHCFT%20%7C%20DEV", "adwks_onqsserver_test", groupId, tenantId, appId, secret);
            store23.Save(db);
        }
    }
}
