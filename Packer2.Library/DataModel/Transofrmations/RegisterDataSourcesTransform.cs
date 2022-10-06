using Microsoft.AnalysisServices.Tabular;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class RegisterDataSourcesTransform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            var partitionExpressions = database.Model.Tables.SelectMany(t => t.Partitions).Select(p => p.Source).OfType<MPartitionSource>();
            var modelExpressions = database.Model.Expressions.Where(e => e.Kind == ExpressionKind.M);

            PullUpSqlServerDataSources(database, partitionExpressions, pe => pe.Expression, (pe, ex) => pe.Expression = ex);
            PullUpSqlServerDataSources(database, modelExpressions, pe => pe.Expression, (pe, ex) => pe.Expression = ex);
            PullUpLocalFileDataSources(database, partitionExpressions, pe => pe.Expression, (pe, ex) => pe.Expression = ex);
            PullUpLocalFileDataSources(database, modelExpressions, pe => pe.Expression, (pe, ex) => pe.Expression = ex);

            return database;
        }

        private static void PullUpLocalFileDataSources<T>(Database database, IEnumerable<T> expressionObjs, Func<T, string> getExpression, Action<T, string> setExpression)
        {
            var fileSources = expressionObjs
                .Select(exp => new { exp, expTxt = getExpression(exp), match = Regex.Match(getExpression(exp), @"Source = (Csv.Document|Excel.Workbook)\(File.Contents\(""(?'path'[^""]+)""\)[^)]*\)", RegexOptions.IgnoreCase) })
                .Where(x => x.match.Success);

            var dsNames = fileSources.Select(fs => fs.match.Groups["path"].Value)
                .Distinct()
                .ToDictionary(
                    path => path,
                    path =>
                    {
                        var extension = Path.GetExtension(path).TrimStart('.');
                        var nameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                        return $"{extension}/{nameWithoutExtension}";
                    });

            foreach (var kvp in dsNames)
            {
                var path = kvp.Key;
                var name = kvp.Value;

                if (database.Model.DataSources.Contains(name))
                    continue;

                var ds = new StructuredDataSource()
                {
                    Name = name,
                    Credential = new Credential() { AuthenticationKind = "Windows", Username = "...", Password = "...", EncryptConnection = false },
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

                database.Model.DataSources.Add(ds);
            }

            foreach (var x in fileSources)
            {
                var name = dsNames[x.match.Groups["path"].Value];
                var newText = x.expTxt.Remove(x.match.Index, x.match.Length).Insert(x.match.Index, @$"Source=#""{name}""");
                setExpression(x.exp, newText);
            }
        }

        private static void PullUpSqlServerDataSources<T>(Database database, IEnumerable<T> expressionObjs, Func<T, string> getExpression, Action<T, string> setExpression)
        {
            var sqlServerSources = expressionObjs
                .Select(exp => new { exp, text = getExpression(exp) })
                .Select(x => new { x.exp, expTxt = x.text, match = Regex.Match(x.text, @"((?'var'Source) = Sql.Database\(""(?'server'[^""]+)"",\s*""(?'database'[^""]+)""\))|(Source = Sql.Databases\(""(?'server'[^""]+)""\),\s+(?'var'[^=]+)=\s*Source{\[Name=""(?'database'[^""]+))""\]}\[Data\]", RegexOptions.IgnoreCase) })
                .Where(x => x.match.Success)
                .Select(x => new { x.match, x.expTxt, x.exp, server = x.match.Groups["server"].Value, database = x.match.Groups["database"].Value, variable = x.match.Groups["var"].Value });

            var dsNames = sqlServerSources.Select(fs => new { fs.server, fs.database })
                .Distinct()
                .ToDictionary(conn => conn, conn =>$"SQL/{conn.server};{conn.database}");

            foreach (var kvp in dsNames)
            {
                var conn = kvp.Key;
                var name = kvp.Value;

                if (database.Model.DataSources.Contains(name))
                    continue;

                var ds = new StructuredDataSource()
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

                database.Model.DataSources.Add(ds);
            }

            foreach (var x in sqlServerSources)
            {
                var name = dsNames[new { x.server, x.database }];
                var newText = x.expTxt.Remove(x.match.Index, x.match.Length).Insert(x.match.Index, $"{x.variable}= #\"{name}\"");
                setExpression(x.exp, newText);
            }
        }
    }
}
