﻿using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.Tools;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class RegisterDataSourcesTransform : IDataModelTransform
    {
        private readonly ILogger<RegisterDataSourcesTransform> logger;

        public RegisterDataSourcesTransform(ILogger<RegisterDataSourcesTransform>? logger = null)
        {
            this.logger = logger ?? new DummyLogger<RegisterDataSourcesTransform>();
        }

        public Database Transform(Database database)
        {
            var partitionExpressions = database.Model.Tables.SelectMany(t => t.Partitions).Select(p => p.Source).OfType<MPartitionSource>();
            var modelExpressions = database.Model.Expressions.Where(e => e.Kind == ExpressionKind.M);

            PullUpSqlServerDataSources(database, partitionExpressions, pe => pe.Expression, (pe, ex) => { pe.Expression = ex; logger.LogInformation("Updating expression in partition '{partitionName}' or table '{tableName}'", pe.Partition.Name, pe.Partition.Table.Name); });
            PullUpSqlServerDataSources(database, modelExpressions, pe => pe.Expression, (pe, ex) => { pe.Expression = ex; logger.LogInformation("Updating expression '{expressionName}'", pe.Name); });
            PullUpLocalFileDataSources(database, partitionExpressions, pe => pe.Expression, (pe, ex) => { pe.Expression = ex; logger.LogInformation("Updating expression in partition '{partitionName}' or table '{tableName}'", pe.Partition.Name, pe.Partition.Table.Name); });
            PullUpLocalFileDataSources(database, modelExpressions, pe => pe.Expression, (pe, ex) => { pe.Expression = ex; logger.LogInformation("Updating expression '{expressionName}'", pe.Name); });

            return database;
        }

        private void PullUpLocalFileDataSources<T>(Database database, IEnumerable<T> expressionObjs, Func<T, string> getExpression, Action<T, string> setExpression)
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
                        var nameWithoutExtension = Path.GetFileNameWithoutExtension(path).Replace(".", " ");
                        return $"{extension}/{nameWithoutExtension}";
                    });

            foreach (var kvp in dsNames)
            {
                var path = kvp.Key;
                var name = kvp.Value;

                if (database.Model.DataSources.Contains(name))
                {
                    logger.LogInformation("Found a reference to a local file data source '{dataSource}' but it's already registered so skipping", name);
                    continue;
                }

                logger.LogInformation("Registering local file data source '{dataSource}'.", name);

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

        private void PullUpSqlServerDataSources<T>(Database database, IEnumerable<T> expressionObjs, Func<T, string> getExpression, Action<T, string> setExpression)
        {
            var sqlServerSources = expressionObjs
                .Select(exp => new { exp, text = getExpression(exp) })
                .Select(x => new { x.exp, expTxt = x.text, match = Regex.Match(x.text, @"((?'var'Source) = Sql.Database\(""(?'server'[^""]+)"",\s*""(?'database'[^""]+)""\))|(Source = Sql.Databases\(""(?'server'[^""]+)""\),\s+(?'var'[^=]+)=\s*Source{\[Name=""(?'database'[^""]+))""\]}\[Data\]", RegexOptions.IgnoreCase) })
                .Where(x => x.match.Success)
                .Select(x => new { x.match, x.expTxt, x.exp, server = x.match.Groups["server"].Value, database = x.match.Groups["database"].Value, variable = x.match.Groups["var"].Value });

            var dsNames = sqlServerSources.Select(fs => new { fs.server, fs.database })
                .Distinct()
                .ToDictionary(conn => conn, conn =>$"SQL/{EscapeServerName(conn.server)};{conn.database.Replace(".", " ")}");

            foreach (var kvp in dsNames)
            {
                var conn = kvp.Key;
                var name = kvp.Value;

                if (database.Model.DataSources.Contains(name))
                {
                    logger.LogInformation("Found a reference to a SQLServer data source '{dataSource}' but it's already registered so skipping", name);
                    continue;
                }

                logger.LogInformation("Registering SQLServer data source '{dataSource}'.", name);

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

        private string EscapeServerName(string server)
        {
            if (server.Trim() == ".")
                return "localhost";
            else
                return server.Replace(".", " ");
        }
    }
}
