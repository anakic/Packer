using Microsoft.AnalysisServices.Tabular;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class DeclareDataSourcesTransform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            var sqlServerSources = database.Model.Tables.SelectMany(t => t.Partitions).Select(p => p.Source).OfType<MPartitionSource>()
                .Select(s => Regex.Match(s.Expression, @"Source = Sql.Database\(""(?'server'[^""]+)"",\s*""(?'database'[^""]+)""\)", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => new { server = m.Groups["server"].Value, database = m.Groups["database"].Value })
                .Distinct()
                .ToList();

            foreach (var conn in sqlServerSources)
            {
                var dsData = new
                {
                    type = "structured",
                    name = $"SQL/{conn.server};{conn.database}",
                    connectionDetails = new
                    {
                        protocol = "tds",
                        address = new
                        {
                            conn.server,
                            conn.database
                        },
                        authentication = null as object,
                        query = null as object
                    },
                    credential = new { }
                };

                database.Model.DataSources.Add(new StructuredDataSource()
                {
                    Name = $"SQL/{conn.server};{conn.database}",
                    ConnectionDetails = new ConnectionDetails($@"{{
                        protocol: ""tds"",
                        address:
                        {{
                            server: ""{conn.server}"",
                            database: ""{conn.database}""
                        }},
                        authentication: null,
                        query: null
                    }}")
                });
            }

            return database;
        }
    }
}
