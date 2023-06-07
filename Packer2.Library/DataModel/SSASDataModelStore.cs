using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api.Models;
using Packer2.Library.Tools;
using System.Data.SqlClient;
using System.Text;
using RefreshType = Microsoft.AnalysisServices.Tabular.RefreshType;

namespace Packer2.Library.DataModel
{
    public enum AutoProcessBehavior
    {
        /// <summary>
        /// The database is not processed automatically after pushing to SSAS.
        /// </summary>
        None,
        /// <summary>
        /// The database is processed automatically after pushing to SSAS. The ordering and parallel processing is controlled by the SSAS instance.
        /// </summary>
        Default,
        /// <summary>
        /// The database is processed automatically after pushing to SSAS. Tables are processed sequentially, one after another (no parallel processing). This can be useful when pushing to PowerBI Service to avoid memory use issues.
        /// </summary>
        Sequential,
    }

    public class SSASDataModelStore : IModelStore<Database>
    {
        SqlConnectionStringBuilder connectionStringBuilder;
        private readonly AutoProcessBehavior autoProcessBehavior;
        private readonly ILogger<SSASDataModelStore> logger;

        public SSASDataModelStore(string connectionString, AutoProcessBehavior autoProcessBehavior = AutoProcessBehavior.Default, ILogger<SSASDataModelStore>? logger = null)
        {
            connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            this.autoProcessBehavior = autoProcessBehavior;
            this.logger = logger ?? new DummyLogger<SSASDataModelStore>();
        }

        public SSASDataModelStore(string server, string? database, AutoProcessBehavior autoProcessBehavior = AutoProcessBehavior.Default, ILogger<SSASDataModelStore>? logger = null)
        {
            connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                // server can be e.g. "localhost:54287"
                DataSource = server,
                InitialCatalog = database
            };

            this.autoProcessBehavior = autoProcessBehavior;
            this.logger = logger ?? new DummyLogger<SSASDataModelStore>();
        }

        public Database Read()
        {
            var s = new Server();
            s.Connect(connectionStringBuilder.ConnectionString);

            var databaseName = connectionStringBuilder.InitialCatalog;
            if (databaseName != null)
                return GetDatabase(s, databaseName);
            else
                return s.Databases[0];
        }

        private static Database? GetDatabase(Server s, string databaseName)
        {
            return s.Databases.OfType<Database>().SingleOrDefault(d => d.ID == databaseName || d.Name == databaseName);
        }

        public void Save(Database database)
        {
            using (var server = new Server())
            {
                logger.LogInformation("Connecting to server '{serverName}'...", connectionStringBuilder.DataSource);
                var stripDbConnStrBuilder = new SqlConnectionStringBuilder(connectionStringBuilder.ConnectionString);
                stripDbConnStrBuilder.InitialCatalog = string.Empty;
                server.Connect(stripDbConnStrBuilder.ConnectionString);

                string databaseName = connectionStringBuilder.InitialCatalog;

                if(!string.Equals(database.ID, databaseName, StringComparison.CurrentCultureIgnoreCase))
                    database.ID = Guid.NewGuid().ToString();

                if (!string.Equals(database.Name, databaseName, StringComparison.CurrentCultureIgnoreCase))
                    database.Name = databaseName;

                var existingDatabase = GetDatabase(server, databaseName);
                if (existingDatabase != null)
                {
                    OnBeforeDeleteOldDb(existingDatabase);

                    // for some weird reason, we must access model this way, if we need it
                    // Microsoft.AnalysisServices.Tabular.Model model = existingDatabase.Model as Microsoft.AnalysisServices.Tabular.Model;
                    logger.LogInformation("Replacing existing database '{databaseName}'...", database.Name);
                    server.Databases.Remove(existingDatabase);
                }
                else
                    logger.LogInformation("Creating database '{databaseName}'...", database.Name);

                server.Databases.Add(database);

                logger.LogInformation("Updating database definition'...");
                database.Update(Microsoft.AnalysisServices.UpdateOptions.ExpandFull, Microsoft.AnalysisServices.UpdateMode.CreateOrReplace);
                
                OnDatabaseUpdated(database);

                try
                {
                    logger.LogInformation("Processing database '{databaseName}'...",  database.Name);

                    // process
                    server.BeginTransaction();
                    if (autoProcessBehavior == AutoProcessBehavior.Default)
                    {
                        
                        logger.LogInformation($"Processing database (populating database data)...");
                        database.Model.RequestRefresh(RefreshType.Full);
                        var results = database.Model.SaveChanges(new SaveOptions() { SaveFlags = SaveFlags.ForceValidation });
                        PrintMessages(results.XmlaResults);
                    }
                    else if (autoProcessBehavior == AutoProcessBehavior.Sequential)
                    {
                        int current = 1;
                        int total = database.Model.Tables.Count;
                        foreach (var table in database.Model.Tables)
                        {
                            logger.LogInformation($"Processing table ({current++}/{total}): '{table.Name}'...");
                            table.RequestRefresh(RefreshType.Full);
                            var results = database.Model.SaveChanges();
                            PrintMessages(results.XmlaResults);
                        }
                    }
                    logger.LogInformation($"Committing changes...");
                    server.CommitTransaction();
                    logger.LogInformation("Processing database '{databaseName}' complete.", database.Name);
                }
                catch(Microsoft.AnalysisServices.OperationException ex)
                {
                    PrintMessages(ex.Results);
                    logger.LogError("Failed to process database '{databaseName}'.", database.Name);
                }
            }
        }

        protected virtual void OnBeforeDeleteOldDb(Database existingDatabase) { }

        protected virtual void OnDatabaseUpdated(Database database) { }

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
