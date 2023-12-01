using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Extensions;
using Microsoft.PowerBI.Api.Models;
using Microsoft.PowerBI.Api.Models.Credentials;
using Microsoft.Rest;
using Packer2.Library.Tools;
using System.Runtime.InteropServices;
using System.Security;

namespace Packer2.Library.DataModel
{
    public class PowerBIServiceDataModelStore : SSASDataModelStore
    {
        private readonly Guid workspaceId;
        private readonly string tenantId;
        private readonly string appId;
        private readonly string appSecret;
        private readonly string? dataSourceUsername;
        private readonly SecureString? dataSourcePassword;
        private readonly ILogger<PowerBIServiceDataModelStore> logger;

        public PowerBIServiceDataModelStore(string workspaceConnection, string databaseName, Guid workspaceId, string tenantId, string appId, string appSecret, AutoProcessBehavior autoProcessBehavior = AutoProcessBehavior.Default, ILogger<PowerBIServiceDataModelStore>? logger = null, string? dataSourceUsername = null, SecureString? dataSourcePassword = null)
            : base($"Server={workspaceConnection};Database={databaseName};User ID=app:{appId}@{tenantId};Password={appSecret};", autoProcessBehavior, logger)
        {
            this.workspaceId = workspaceId;
            this.tenantId = tenantId;
            this.appId = appId;
            this.appSecret = appSecret;
            this.logger = logger ?? new DummyLogger<PowerBIServiceDataModelStore>();
            this.dataSourceUsername = dataSourceUsername;
            this.dataSourcePassword = dataSourcePassword;
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

                // fetch the gateway to use
                var allGateways = (await client.Gateways.GetGatewaysAsync()).Value;
                var gw = string.IsNullOrEmpty(GatewayName)
                    ? allGateways.Single()
                    : allGateways.Single(x => x.Name == GatewayName);

                var dataSources = await client.Gateways.GetDatasourcesAsync(gw.Id);

                // Go through the model datasources and find one which has type of structured. Extract the ConnectionDetails and compare to the ds2 list to see if any have the same connection details
                var modelDatasource = database.Model.DataSources.Single(x => x.Type == DataSourceType.Structured) as Microsoft.AnalysisServices.Tabular.StructuredDataSource;

                if (modelDatasource == null)
                {
                    throw new System.Exception("No structured datasource found in model");
                }
                // Create a connstring for matching which should be like {"server":"dsp","database":"NS-FullServiceTest"}
                string databaseName = modelDatasource.ConnectionDetails.Address.Database;
                string dsConnString = $"{{\"server\":\"{modelDatasource.ConnectionDetails.Address.Server}\",\"database\":\"{databaseName}\"}}";
                // Try and find a match in the ds2 list, if there is no match then create a new datasource and bind to the gateway
                var ds2Match = dataSources.Value.FirstOrDefault(x => x.ConnectionDetails == dsConnString);

                // If there is a match, then bind the dataset to the gateway
                if (ds2Match != null)
                {
                    List<Guid?> ds2MatchList = new List<Guid?>();
                    ds2MatchList.Add(ds2Match.Id);

                    await client.Datasets.BindToGatewayInGroupAsync(workspaceId, datasetId, new BindToGatewayRequest(gw.Id));
                }
                else
                {
                    // If there was no matching datasource to use then a new one needs creating
                    if (string.IsNullOrEmpty(dataSourceUsername))
                    {
                        throw new Exception("No matching data source can be found. Data source credentials are needed to create a new one, please run cmdlet with them provided");
                    }
                    // This segment was sourced from code on: https://learn.microsoft.com/en-us/power-bi/developer/embedded/configure-credentials?tabs=sdk3
                    var credentials = new WindowsCredentials(username: dataSourceUsername, password: SecureStringToString(dataSourcePassword));
                    var credentialsEncryptor = new AsymmetricKeyEncryptor(gw.PublicKey);
                    var credentialDetails = new CredentialDetails(credentials, PrivacyLevel.Private, EncryptedConnection.Encrypted, credentialsEncryptor);
                    string connection = dsConnString;
                    var request = new PublishDatasourceToGatewayRequest(
                        dataSourceType: "SQL",
                        connectionDetails: connection,
                        credentialDetails: credentialDetails,
                        dataSourceName: databaseName + "_DataSource");

                    var newDatasource = await client.Gateways.CreateDatasourceAsync(gw.Id, request);

                    List<Guid?> newDSList = new List<Guid?>();
                    newDSList.Add(newDatasource.Id);

                    await client.Datasets.BindToGatewayInGroupAsync(workspaceId, datasetId, new BindToGatewayRequest(gw.Id, newDSList));
                }
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

        // To keep the password as a securestring for as long as possible only convert at the point it needs to become clear text for credentials
        private string SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
