using LibGit2Sharp;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.PowerBI.Api.Models.Credentials;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.ManualTestingArea
{
    internal class Class3
    {
        public static async Task Main()
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

            var datasetId = Guid.Parse("20c55c03-19ac-44a0-a02a-dc53345a4b3f");
            var groupId = Guid.Parse("30b80ed2-a558-4165-ad3b-60e2cec50e4b");
            using (var client = new PowerBIClient(new Uri("https://api.powerbi.com"), tokenCredentials))
            {
                // Replace 'groupId' and 'datasetId' with your actual group and dataset IDs
                var datasources = await client.Datasets.GetDatasourcesInGroupAsync(groupId, datasetId.ToString());

                var datasource = datasources.Value[0];
                
                var sqluser = "thingieq_abc";
                var sqlPassword = "M6yrKIi^K4FQh12O";
                var updateDataSourceRequest = new UpdateDatasourceRequest
                {
                    CredentialDetails = new CredentialDetails
                    {
                        CredentialType = "Basic",
                        Credentials = "{\"credentialData\":[{\"name\":\"username\", \"value\":\"" + sqluser + "\"},{\"name\":\"password\", \"value\":\"" + sqlPassword + "\"}]}",
                        EncryptedConnection = "Encrypted",
                        EncryptionAlgorithm = "None",
                        PrivacyLevel = "None"

                    }
                };
                //await client.Gateways.UpdateDatasourceAsync(datasource.GatewayId.Value, datasource.DatasourceId.Value, updateDataSourceRequest);
                await client.Gateways.UpdateDatasourceAsync(datasource.GatewayId.Value, datasource.DatasourceId.Value, updateDatasourceRequest: updateDataSourceRequest);
            }

        }
    }
}
