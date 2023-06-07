using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel
{
    public class PowerBIServiceDataModelStore : SSASDataModelStore
    {
        private readonly Guid workspaceId;
        private readonly string tenantId;
        private readonly string appId;
        private readonly string appSecret;
        private readonly ILogger<PowerBIServiceDataModelStore> logger;

        public PowerBIServiceDataModelStore(string workspaceConnection, string databaseName, Guid workspaceId, string tenantId, string appId, string appSecret, AutoProcessBehavior autoProcessBehavior = AutoProcessBehavior.Default, ILogger<PowerBIServiceDataModelStore>? logger = null)
            : base($"Server={workspaceConnection};Database={databaseName};User ID=app:{appId}@{tenantId};Password={appSecret};", autoProcessBehavior, logger)
        {
            this.workspaceId = workspaceId;
            this.tenantId = tenantId;
            this.appId = appId;
            this.appSecret = appSecret;
            this.logger = logger ?? new DummyLogger<PowerBIServiceDataModelStore>();
        }

        public string GatewayName { get; set; }

        protected override void OnBeforeDeleteOldDb(Database existingDatabase)
        {
            logger.LogInformation("Ensuring ownership of existing database {existingDatabase}", existingDatabase.Name);
            DoPbi(async client => 
            {
                await client.Datasets.TakeOverInGroupAsync(workspaceId, existingDatabase.ID);
            }).Wait();
        }

        protected override void OnDatabaseUpdated(Database database)
        {
            logger.LogInformation("Binding dataset to gateway");
            DoPbi(async client =>
            {
                var datasetId = database.ID;

                // Get the datasource from your dataset
                var datasource = await client.Datasets.GetDatasourcesInGroupAsync(workspaceId, datasetId);

                // fetch the gateway to use
                var allGateways = (await client.Gateways.GetGatewaysAsync()).Value;
                var gw = string.IsNullOrEmpty(GatewayName) 
                    ? allGateways.Single()
                    : allGateways.Single(x => x.Name == GatewayName);

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
}
