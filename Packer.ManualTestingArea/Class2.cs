using Microsoft.AnalysisServices.Tabular;
using Microsoft.Identity.Client;
using Packer2.Library;
using Packer2.Library.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.ManualTestingArea
{
    public class Class2
    {
        public static void Main_()
        {
            var appId = "2f11d9ca-a570-40c9-a41a-ba3834735799";
            var tenantId = "3b6e5b83-9337-4698-9e67-cc3d722cd837";
            var secret = "5168Q~potHnmmlv5ooZJFCcFlESCuI5Io~6GzaZG";

            FolderDatabaseStore storeLocal = new FolderDatabaseStore("C:\\models\\aw_online_customized");
            var db = storeLocal.Read();

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
    }
}
