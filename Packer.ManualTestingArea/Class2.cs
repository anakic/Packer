using Microsoft.AnalysisServices.Tabular;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api.Extensions;
using Microsoft.PowerBI.Api.Models.Credentials;
using Microsoft.PowerBI.Api.Models;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using Packer2.Library;
using Packer2.Library.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.AnalysisServices;

namespace Packer.ManualTestingArea
{
    public class Class2
    {
        public static async Task Main()
        {
            //Upload();
            await Bind();
            // await Adjust(Guid.Parse("3e4364ec-47f8-411a-b3c4-a397eaef1bfe"));
            // Upload();
        }

        private static async Task Bind()
        {
            await DoPbi(async client =>
            {
                var groupId = Guid.Parse("30b80ed2-a558-4165-ad3b-60e2cec50e4b");
                var datasetId = "288c8219-4dec-4e1b-89a6-f82169a26e0a";

                var ds = await client.Datasets.GetDatasourcesInGroupAsync(groupId, datasetId);

                // Get the datasource from your dataset
                var datasource = await client.Datasets.GetDatasourcesInGroupAsync(groupId, datasetId);
                var gw = (await client.Gateways.GetGatewaysAsync()).Value.Single();

                var ds2 = await client.Gateways.GetDatasourcesAsync(gw.Id);

                await client.Datasets.BindToGatewayInGroupAsync(groupId, datasetId, new BindToGatewayRequest(gw.Id, new List<Guid?>() { ds2.Value.First().Id } ));
            });
        }

        public static void Upload()
        {
            var appId = "2f11d9ca-a570-40c9-a41a-ba3834735799";
            var tenantId = "3b6e5b83-9337-4698-9e67-cc3d722cd837";
            var secret = "5168Q~potHnmmlv5ooZJFCcFlESCuI5Io~6GzaZG";

            var db = TabularModel.LoadFromPbitFile("C:\\Users\\anton\\OneDrive\\Documents\\aw_pbi_test.pbit");
            db.DeclareDataSources();

            var store23 = new SSASDataModelStore($"Server=powerbi://api.powerbi.com/v1.0/myorg/PremiumPU;Database=adwks_onqsserver_test;User ID=app:{appId}@{tenantId};Password={secret};");
            store23.Save(db);



            // Read - TabularModel.\sadf | Write - TabularModel.\with_root_ignores2 - Customization wlt1

            //FolderDatabaseStore store = new FolderDatabaseStore(@"C:\Users\AntonioNakic_lbehpwm\OneDrive - RealWorld.Health\Desktop\Data Model");
            //var model = store.Read();

            //FolderDatabaseStore store2 = new FolderDatabaseStore("C:\\models\\test234\\with_cust", "WLT");
            //store2.Save(model);


            //var appId = "2f11d9ca-a570-40c9-a41a-ba3834735799";
            //var tenantId = "3b6e5b83-9337-4698-9e67-cc3d722cd837";
            //var secret = "5168Q~potHnmmlv5ooZJFCcFlESCuI5Io~6GzaZG";

            //var authority = $"https://login.microsoftonline.com/{tenantId}";
            //var scopes = new string[] { "https://analysis.windows.net/powerbi/api/.default" };

            //var app = ConfidentialClientApplicationBuilder
            //    .Create(appId)
            //    .WithClientSecret(secret)
            //    .WithAuthority(new Uri(authority))
            //    .Build();

            //var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            //FolderDatabaseStore storeLocal = new FolderDatabaseStore("C:\\models\\aw_online_customized");
            //var db = storeLocal.Read();


            //string server = "powerbi://api.powerbi.com/v1.0/myorg/PremiumPU";
            //string token = authResult.AccessToken;
            //string database = "adwks_onqsserver_test";
            //var connStr = $"Data Source={server};Password={token};Initial Catalog={database};Persist Security Info=True;";

            //var store23 = new SSASDataModelStore(connStr);
            //store23.Save(db);
        }

        static async Task DoPbi(Func<PowerBIClient, Task> action)
        {
            var appId = "2f11d9ca-a570-40c9-a41a-ba3834735799";
            var tenantId = "3b6e5b83-9337-4698-9e67-cc3d722cd837";
            var secret = "5168Q~potHnmmlv5ooZJFCcFlESCuI5Io~6GzaZG";

            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var scopes = new string[] { "https://analysis.windows.net/powerbi/api/.default" };

            var app = ConfidentialClientApplicationBuilder
                .Create(appId)
                .WithClientSecret(secret)
                .WithAuthority(new Uri(authority))
                .Build();

            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            var tokenCredentials = new TokenCredentials(authResult.AccessToken, "Bearer");

            using (var client = new PowerBIClient(new Uri("https://api.powerbi.com"), tokenCredentials))
            {
                await action(client);
            }
        }

        public static async Task Adjust(Guid guid)
        {
            var datasetId = guid;
            var groupId = Guid.Parse("30b80ed2-a558-4165-ad3b-60e2cec50e4b");
            await DoPbi(async client =>
            {
                // Replace 'groupId' and 'datasetId' with your actual group and dataset IDs
                var datasources = await client.Datasets.GetDatasourcesInGroupAsync(groupId, datasetId.ToString());

                var datasource = datasources.Value[0];

                var sqluser = "thingieq_abc";
                var sqlPassword = "M6yrKIi^K4FQh12O";

                var xx = client.Gateways.GetGateways();
                client.Datasets.BindToGateway(datasetId.ToString(), new BindToGatewayRequest(xx.Value.First().Id));

                var credentials = new BasicCredentials(username: sqluser, password: sqlPassword);
                var gateway = client.Gateways.GetGateway(datasource.GatewayId.Value);
                var credentialsEncryptor = new AsymmetricKeyEncryptor(gateway.PublicKey);
                var credentialDetails = new CredentialDetails(
                    credentials,
                    PrivacyLevel.Private,
                    EncryptedConnection.Encrypted,
                    credentialsEncryptor);

                var updateDataSourceRequest = new UpdateDatasourceRequest(credentialDetails);

                //await client.Gateways.UpdateDatasourceAsync(datasource.GatewayId.Value, datasource.DatasourceId.Value, updateDataSourceRequest);
                await client.Gateways.UpdateDatasourceAsync(datasource.GatewayId.Value, datasource.DatasourceId.Value, updateDatasourceRequest: updateDataSourceRequest);
            });
        }
    }
}
