using Microsoft.AnalysisServices.Tabular;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class ExportDataSourcesTransform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            var expressions = database.Model.Tables.SelectMany(t => t.Partitions).Select(p => p.Source).OfType<MPartitionSource>()
                .Select(s => s.Expression)
                .Union(database.Model.Expressions.Where(e => e.Kind == ExpressionKind.M).Select(x => x.Expression))
                .ToList();

            /*arbitrary name and compatibility level (because they always get serialized, even if not specified so might as well specify)*/
            var exportDatabase = new Database() { Name = "Data sources", CompatibilityLevel = 1400, Model = new Model() };
            var sqlSevDataSources = FindSqlServerDataSources(database, expressions);
            var localFileDataSources = FindLocalFileDataSources(database, expressions);

            foreach (var ds in sqlSevDataSources.Concat(localFileDataSources))
                exportDatabase.Model.DataSources.Add(ds);

            return exportDatabase;
        }

        private static IEnumerable<DataSource> FindLocalFileDataSources(Database database, List<string> expressions)
        {
            var fileSources = expressions
                .Select(exp => Regex.Match(exp, @"Source = (Csv.Document|Excel.Workbook)\(File.Contents\(""(?'path'[^""]+)""", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => m.Groups["path"].Value)
                .Distinct()
                .ToList();

            foreach (var path in fileSources)
            {
                var extension = Path.GetExtension(path).TrimStart('.');
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(path);

                var name = $"{extension}/{nameWithoutExtension}";
                if (database.Model.DataSources.Contains(name))
                    continue;

                yield return new StructuredDataSource()
                {
                    Name = name,
                    Credential = new Credential() { AuthenticationKind = "UsernamePassword", Username="...", Password="...", EncryptConnection=false },
                    ConnectionDetails = new ConnectionDetails($@"{{
                        protocol: ""file"",
                        address:
                        {{
                            path: {Newtonsoft.Json.JsonConvert.SerializeObject(path)},
                        }},
                        authentication: null,
                        query: null
                    }}")
                };
            }
        }

        private static IEnumerable<DataSource> FindSqlServerDataSources(Database database, List<string> expressions)
        {
            var sqlServerSources = expressions
                .Select(e => Regex.Match(e, @"(Source = Sql.Database\(""(?'server'[^""]+)"",\s*""(?'database'[^""]+)""\))|(Source = Sql.Databases\(""(?'server'[^""]+)""[^=]*=\s*Source{\[Name=""(?'database'[^""]+))", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => new { server = m.Groups["server"].Value, database = m.Groups["database"].Value })
                .Distinct()
                .ToList();

            foreach (var conn in sqlServerSources)
            {
                var name = $"SQL/{conn.server};{conn.database}";
                if (database.Model.DataSources.Contains(name))
                    continue;

                yield return new StructuredDataSource()
                {
                    Name = name,
                    Credential = new Credential() { AuthenticationKind = "UsernamePassword", Username = "...", Password = "...", EncryptConnection = false },
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
                };
            }
        }
    }
}
