using Microsoft.AnalysisServices.Tabular;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataModelLoader.Transofrmations
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
                            server = conn.server,
                            database = conn.database
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

            /*

            var fileSources = expressions.Select(e => Regex.Match(e.Value<string>()!,
                @"Source = (Csv.Document|Excel.Workbook)\(File.Contents\(""(?'path'[^""]+)""", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => m.Groups["path"].Value)
                .ToHashSet();
            foreach (var path in fileSources)
            {
                var extension = Path.GetExtension(path).TrimStart('.');
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(path);

                var dsData = new
                {
                    type = "structured",
                    name = $"{extension}/{nameWithoutExtension}",
                    connectionDetails = new
                    {
                        protocol = "file",
                        address = new
                        {
                            path = $"{path}"
                        },
                        authentication = null as object,
                        query = null as object
                    },
                    credential = new { }
                };

                dataSourceElement.Add(JObject.FromObject(dsData));
            }

            (obj["model"] as JObject)!.Add("dataSources", dataSourceElement);
            */
        }
    }
}
