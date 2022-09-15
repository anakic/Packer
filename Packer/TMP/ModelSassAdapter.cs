using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Packer.TMP
{
    class ModelSassAdapter : IModelTransform<DataModel>
    {
        public void Transform(DataModel model)
        {
            var obj = model.JObject;

            Strip(obj);
            FixCoalesce(obj);
            GenerateDataSources(obj);
            ExtractMQueries(obj);
            SetCompatibilityLevel(obj);
        }

        private void FixCoalesce(JObject obj)
        {
            var expressions1 = obj.SelectTokens("model.tables[*].columns[?(@.type=='calculated' && @.expression)]")
                .OfType<JObject>()
                .Where(jo => jo["expression"]!.Value<string>()!.ToLower().Contains("coalesce"));

            var expressions2 = obj.SelectTokens("model.tables[*].measures[*]")
                .OfType<JObject>()
                .Where(jo => jo["expression"]!.Value<string>()!.ToLower().Contains("coalesce"));

            expressions1.Concat(expressions2).ToList().ForEach(e =>
            {
                // tmp - todo: antlr
                e["expression"] = "1";
            });
        }

        private void GenerateDataSources(JObject obj)
        {
            var expressions = obj.SelectTokens("model.tables[*].partitions[?(@.source.type=='m')].source.expression");

            var dataSourceElement = new JArray();

            var dbConnections = expressions.Select(e => Regex.Match(e.Value<string>()!,
                @"Source = Sql.Database\(""(?'server'[^""]+)"",\s*""(?'database'[^""]+)""\)", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => new { server = m.Groups["server"].Value, database = m.Groups["database"].Value })
                .ToHashSet();
            foreach (var conn in dbConnections)
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

                dataSourceElement.Add(JObject.FromObject(dsData));
            }

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
        }

        private void SetCompatibilityLevel(JObject obj)
            => obj["compatibilityLevel"] = 1500;

        private void ExtractMQueries(JObject obj)
        {
            var globalExpressionsNode = obj.SelectTokens("$.model.expressions").OfType<JArray>().SingleOrDefault();
            if (globalExpressionsNode == null)
            {
                globalExpressionsNode = new JArray();
                (obj.SelectToken("$.model") as JObject)!.Add("expressions", globalExpressionsNode);
            };

            var expressions = obj.SelectTokens("model.tables[*].partitions[?(@.source.type=='m')].source.expression");
            foreach (var exp in expressions)
            {
                var tableName = exp.Parent!.Parent!.Parent!.Parent!.Parent!.Parent!.Parent!["name"];
                var expressionText = exp.Value<string>();

                var expObj = new JObject();
                expObj["name"] = tableName;
                expObj["kind"] = "m";
                expObj["expression"] = expressionText;
                (exp.Parent as JProperty)!.Value =
        $@"let
	Source = #""{tableName}""
in
  Source";
                globalExpressionsNode.Add(expObj);
            }
        }

        private void Strip(JObject obj)
        {
            // strip properties
            var propsToStrip = new[] {
        "createdTimestamp", "lastUpdate", "modifiedTime", "lastSchemaUpdate", "lastProcessed",
        "lineageTag", "sourceQueryCulture", "defaultPowerBIDataSourceVersion",
        "variations", "structureModifiedTime", "refreshedTime", "attributeHierarchy"
    };
            foreach (var p in propsToStrip)
                obj.SelectTokens(@$"..{p}").Select(x => x.Parent).RemoveTokens();

            // string rowNumber columns
            obj.SelectTokens(@"$.model..columns[?(@.type == 'rowNumber')]").RemoveTokens();

            obj.SelectTokens(@"$.model.cultures[*]").OfType<JObject>().RemoveTokens();

            var tableNamePatternsToStripOut = new[] { @"^DateTableTemplate_\w+", @"^LocalDateTable_\w+" };
            obj.SelectTokens(@"$.model..tables[*]").OfType<JObject>()
                .Where(t => tableNamePatternsToStripOut.Any(p => Regex.IsMatch(t["name"]!.ToString(), p)))
                .RemoveTokens();

            obj.SelectTokens(@"$.model..relationships[*]").OfType<JObject>()
                .Where(t => tableNamePatternsToStripOut.Any(p => Regex.IsMatch(t["fromTable"]!.ToString(), p) || Regex.IsMatch(t["toTable"].ToString(), p)))
                .RemoveTokens();
        }
    }

    static class JsonHelperExtensions
    {
        public static void RemoveTokens(this IEnumerable<JToken?> tokens)
            => tokens.ToList().ForEach(t => t.Remove());
    }
}
