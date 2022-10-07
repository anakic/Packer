using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.DataModel.Transofrmations;
using System.Data.SqlClient;

namespace Packer2.Library.DataModel
{
    public class SSASDataModelStore : IDataModelStore
    {
        private readonly string serverName;
        private readonly string? databaseName;
        private readonly bool processOnSave;
        private readonly ILogger<SSASDataModelStore>? logger;

        public SSASDataModelStore(string server, string? database, bool processOnSave = true, ILogger<SSASDataModelStore>? logger = null)
        {
            // server can be e.g. "localhost:54287"
            this.serverName = server;
            this.databaseName = database;
            this.processOnSave = processOnSave;
            this.logger = logger ?? new DummyLogger<SSASDataModelStore>();
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

                try
                {
                    // process
                    server.BeginTransaction();
                    if (processOnSave)
                        database.Model.RequestRefresh(RefreshType.Full);
                    var results = database.Model.SaveChanges(new SaveOptions() { SaveFlags = SaveFlags.ForceValidation });
                    server.CommitTransaction();
                    PrintMessages(results.XmlaResults);
                }
                catch(Microsoft.AnalysisServices.OperationException ex)
                {
                    PrintMessages(ex.Results);
                }
            }
        }

        private void PrintMessages(Microsoft.AnalysisServices.XmlaResultCollection results)
        {
            var messages = results.OfType<Microsoft.AnalysisServices.XmlaResult>().SelectMany(r => r.Messages.OfType<Microsoft.AnalysisServices.XmlaMessage>());
            foreach (var m in messages)
            {
                var x = m.Location.SourceObject;
                if (m is Microsoft.AnalysisServices.XmlaWarning)
                {
                    // todo: can also get error positions (start index and length) for dax errors
                    logger.LogWarning("{location}: {message}", LocationStr(m.Location?.SourceObject), m.Description);
                }
                else if (m is Microsoft.AnalysisServices.XmlaError)
                {
                    logger.LogError("{location}: {message}", LocationStr(m.Location?.SourceObject), m.Description);
                }
                else
                    logger.LogError("Packer to-do: don't know how to display message {message}", m);
            }
        }

        private string LocationStr(Microsoft.AnalysisServices.XmlaLocationReference? locationRef)
        {
            if (locationRef == null)
                return "<Unknown location>";
            return $"{locationRef.TableName }[{locationRef.ColumnName ?? locationRef.MeasureName}]";
        }
    }
}
