using Microsoft.AnalysisServices.Tabular;
using System.Data.SqlClient;

namespace Packer2.Library.DataModel
{
    public class SSASDataModelStore : IDataModelStore
    {
        private readonly string serverName;
        private readonly string? databaseName;
        private readonly bool processOnSave;

        public SSASDataModelStore(string server, string? database, bool processOnSave = true)
        {
            // server can be e.g. "localhost:54287"
            this.serverName = server;
            this.databaseName = database;
            this.processOnSave = processOnSave;
        }

        public Database Read()
        {
            var s = new Server();
            s.Connect($"Data source={serverName}");
            var info = s.ConnectionInfo;

            if (databaseName != null)
                return s.Databases[databaseName];
            else
                return s.Databases[0];
        }

        public void Save(Database database)
        {
            // todo: implement
            // - start a local ssas server that I have rights to (docker or local)
            // - test save (if the model is already connected to the server/datase that were specified in the ctor)
            // - if the server is empty, or the server/db are not the same as passed in ctor, create a new db
            //      a) use the update method with UpdateMode.Create (or use UpdateOrCreate with the correct server) https://docs.microsoft.com/en-us/analysis-services/tom/create-and-deploy-an-empty-database-analysis-services-amo-tom?view=asallproducts-allversions
            //      b) see decomiped TabularEditor code: TabularEditor.TOMWrapper.Utils.TabularDeployer (TOMWrapper14.dll) - it uses tsml to create the model

            using (var server = new Server())
            {
                var builder = new SqlConnectionStringBuilder();
                builder.DataSource = serverName;
                server.Connect(builder.ConnectionString);

                database.ID = database.Name = databaseName;

                if (server.Databases.Contains(database.Name))
                    server.Databases.Remove(database.Name);

                server.Databases.Add(database);
                database.Update(Microsoft.AnalysisServices.UpdateOptions.ExpandFull, Microsoft.AnalysisServices.UpdateMode.CreateOrReplace);

                // process
                server.BeginTransaction();
                if(processOnSave)
                    database.Model.RequestRefresh(RefreshType.Full);
                var results = database.Model.SaveChanges(new SaveOptions() { SaveFlags = SaveFlags.ForceValidation });
                server.CommitTransaction();

                foreach (var msg in results.XmlaResults.OfType<Microsoft.AnalysisServices.XmlaResult>().SelectMany(r => r.Messages.OfType<Microsoft.AnalysisServices.XmlaMessage>()))
                {
                    // log warnings
                }
            }
        }
    }
}
