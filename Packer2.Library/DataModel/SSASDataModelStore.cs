using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.Tools;
using System.Data.SqlClient;
using System.Text;

namespace Packer2.Library.DataModel
{
    public class SSASDataModelStore : IModelStore<Database>
    {
        private readonly string serverName;
        private readonly string? databaseName;
        private readonly bool processOnSave;
        private readonly ILogger<SSASDataModelStore> logger;

        public SSASDataModelStore(string connectionString, bool processOnSave = true, ILogger<SSASDataModelStore>? logger = null)
        {
            var connStrBuilder = new SqlConnectionStringBuilder(connectionString);

            this.serverName = connStrBuilder.DataSource;
            this.databaseName = connStrBuilder.InitialCatalog;
            this.processOnSave = processOnSave;
            this.logger = logger ?? new DummyLogger<SSASDataModelStore>();
        }

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

                logger.LogInformation("Connecting to server '{serverName}'...", serverName);
                server.Connect(builder.ConnectionString);

                if(!string.Equals(database.ID, databaseName, StringComparison.CurrentCultureIgnoreCase))
                    database.ID = databaseName;

                if (!string.Equals(database.Name, databaseName, StringComparison.CurrentCultureIgnoreCase))
                    database.Name = databaseName;

                if (server.Databases.Contains(database.Name))
                {
                    logger.LogInformation("Replacing existing database '{databaseName}'...", database.Name);
                    server.Databases.Remove(database.Name);
                }
                else
                    logger.LogInformation("Creating database '{databaseName}'...", database.Name);

                server.Databases.Add(database);
                database.Update(Microsoft.AnalysisServices.UpdateOptions.ExpandFull, Microsoft.AnalysisServices.UpdateMode.CreateOrReplace);

                try
                {
                    logger.LogInformation("Processing database '{databaseName}'...", database.Name);

                    // process
                    server.BeginTransaction();
                    if (processOnSave)
                        database.Model.RequestRefresh(RefreshType.Full);
                    var results = database.Model.SaveChanges(new SaveOptions() { SaveFlags = SaveFlags.ForceValidation });
                    server.CommitTransaction();
                    PrintMessages(results.XmlaResults);
                    logger.LogInformation("Processing database '{databaseName}' complete.", database.Name);
                }
                catch(Microsoft.AnalysisServices.OperationException ex)
                {
                    PrintMessages(ex.Results);
                    logger.LogError("Failed to process database '{databaseName}'.", database.Name);
                }
            }
        }

        private void PrintMessages(Microsoft.AnalysisServices.XmlaResultCollection results)
        {
            var messages = results.OfType<Microsoft.AnalysisServices.XmlaResult>().SelectMany(r => r.Messages.OfType<Microsoft.AnalysisServices.XmlaMessage>());
            foreach (var m in messages)
            {
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

            StringBuilder sb = new StringBuilder("<");
            if (!string.IsNullOrEmpty(locationRef.TableName))
                sb.Append($"nameof({locationRef.TableName}):{locationRef.TableName}; ");
            if (!string.IsNullOrEmpty(locationRef.PartitionName))
                sb.Append($"nameof({locationRef.PartitionName}):{locationRef.PartitionName}; ");
            if (!string.IsNullOrEmpty(locationRef.Role))
                sb.Append($"nameof({locationRef.Role}):{locationRef.Role}; ");
            if (!string.IsNullOrEmpty(locationRef.RoleName))
                sb.Append($"nameof({locationRef.RoleName}):{locationRef.RoleName}; ");
            if (!string.IsNullOrEmpty(locationRef.Dimension))
                sb.Append($"nameof({locationRef.Dimension}):{locationRef.Dimension}; ");
            if (!string.IsNullOrEmpty(locationRef.ColumnName))
                sb.Append($"nameof({locationRef.ColumnName}):{locationRef.ColumnName}; ");
            if (!string.IsNullOrEmpty(locationRef.CalculationItemName))
                sb.Append($"nameof({locationRef.CalculationItemName}):{locationRef.CalculationItemName}; ");
            if (!string.IsNullOrEmpty(locationRef.MeasureGroup))
                sb.Append($"nameof({locationRef.MeasureGroup}):{locationRef.MeasureGroup}; ");
            if (!string.IsNullOrEmpty(locationRef.MeasureName))
                sb.Append($"nameof({locationRef.MeasureName}):{locationRef.MeasureName}; ");
            if (!string.IsNullOrEmpty(locationRef.Attribute))
                sb.Append($"nameof({locationRef.Attribute}):{locationRef.Attribute}; ");
            if (!string.IsNullOrEmpty(locationRef.Hierarchy))
                sb.Append($"nameof({locationRef.Hierarchy}):{locationRef.Hierarchy}; ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(">");

            return sb.ToString();
        }
    }
}
