using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel
{
    public class FolderModelStore : IDataModelStore
    {
        PathEscaper pathEscaper = new PathEscaper();

        class JObjFile : ITextFile
        {
            public JObject JObject { get; private set; }

            public JObjFile(JObject jobj = null)
            {
                JObject = jobj;
            }

            public string GetText()
            {
                return JObject.ToString();
            }

            public void SetText(string text)
            {
                JObject = JObject.Parse(text);
            }
        }

        string folder;
        public FolderModelStore(string folder)
        {
            this.folder = folder;
        }

        public Database Read()
        {
            var root = JObject.Parse(File.ReadAllText(Path.Combine(folder, "database.json")));
            var tablesArr = root.SelectToken(".model.tables") as JArray;
            foreach (var tableFolder in Directory.GetDirectories(Path.Combine(folder, "Tables")))
            {
                var tableObj = JObject.Parse(File.ReadAllText(Path.Combine(tableFolder, "table.json")));

                foreach (var expToken in tableObj.SelectTokens("..expression"))
                {
                    var val = expToken.Value<string>();
                    var m = Regex.Match(val, @"{ref:\s*(?'path'[^}]+)}");
                    if (m.Success)
                    {
                        var expression = File.ReadAllText(Path.Combine(tableFolder, m.Groups["path"].Value));
                        (expToken.Parent as JProperty).Value = expression;
                    }
                }

                tablesArr.Add(tableObj);
            }

            // todo: this could be a static member
            var extensionToExtTypeMap = expTypeToExtensionsMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            var expsArr = root.SelectToken(".model.expressions") as JArray;
            if (expsArr != null)
            {
                foreach (var node in expsArr)
                {
                    var val = node["expression"].ToString();
                    var m = Regex.Match(val, @"{ref:\s*(?'path'[^}]+)}");
                    if (m.Success)
                    {
                        var expression = File.ReadAllText(Path.Combine(folder, m.Groups["path"].Value));
                        node["expression"] = expression;
                    }
                }
            }
            var file = new JObjFile(root);
            var inner = new BimDataModelStore(file);
            return inner.Read();
        }

        // todo: make a common base class for this and ReportFolderStore that has this method and calls it on Save()
        // Currently this is copy-pasted from there.
        private void ClearFolder()
        {
            if (Directory.Exists(folder))
            {
                foreach (var childDir in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    // do not remove the .git folder
                    if (Path.GetFileName(childDir) != ".git")
                        Directory.Delete(childDir, true);
                }

                foreach (var file in Directory.GetFiles(folder))
                    File.Delete(file);
            }
        }

        public void Save(Database model)
        {
            ClearFolder();

            var jObjFile = new JObjFile();
            var inner = new BimDataModelStore(jObjFile);
            inner.Save(model);
            var jobj = jObjFile.JObject;
            ExpandTables(jobj, Path.Combine(folder, "Tables"));
            ExpandExpressions(jobj.SelectTokens(".model.expressions[*]"), folder);
            WriteToFile(Path.Combine(folder, "database.json"), jobj);
        }

        private void ExpandExpressions(IEnumerable<JToken> nodes, string folderPath)
        {
            foreach (var expNode in nodes)
            {
                var name = expNode["name"]!.Value<string>()!;
                var type = expNode["kind"]!.Value<string>()!;
                var ext = expTypeToExtensionsMap[type];
                var exp = expNode.SelectToken("expression")!.Value<string>();
                var relativePath = Path.Combine("Expressions", $"{pathEscaper.EscapeName(name)}.{ext}");
                WriteToFile(Path.Combine(folderPath, relativePath), exp);
                expNode["expression"] = $"{{ref: {relativePath}}}";
            }
        }

        private void ExpandTables(JObject jobj, string path)
        {
            foreach (var tok in jobj.SelectTokens(".model.tables[*]").ToArray())
            {
                string name = tok["name"].Value<string>();
                var tableFolderPath = Path.Combine(path, pathEscaper.EscapeName(name));
                RefDaxExpressions(tok.SelectTokens("columns[?(@.type=='calculated' && @.expression != null)]"), tableFolderPath, "ColumnExpressions");
                RefDaxExpressions(tok.SelectTokens("measures[*]"), tableFolderPath, "MeasureExpressions");
                RefPartitionExpressions(tok.SelectTokens("partitions[*]"), tableFolderPath, "PartitionExpressions");
                WriteToFile(Path.Combine(tableFolderPath, $"table.json"), tok);
                tok.Remove();
            }
        }

        static Dictionary<string, string> expTypeToExtensionsMap = new Dictionary<string, string>() { { "m", "m" }, { "calculated", "dax" } };
        private void RefPartitionExpressions(IEnumerable<JToken> partitionTokens, string rootFolderPath, string subFolderPath)
        {
            foreach (var mesTok in partitionTokens)
            {
                var name = mesTok["name"].Value<string>();
                var type = mesTok.SelectToken("source.type").Value<string>();
                var ext = expTypeToExtensionsMap[type];
                var exp = mesTok.SelectToken("source.expression").Value<string>();
                var relativePath = Path.Combine(subFolderPath, $"{pathEscaper.EscapeName(name)}.{ext}");
                WriteToFile(Path.Combine(rootFolderPath, relativePath), exp);
                mesTok.SelectToken("source")["expression"] = $"{{ref: {relativePath}}}";
            }
        }

        private void RefDaxExpressions(IEnumerable<JToken> measureTokens, string rootFolderPath, string subFolderPath)
        {
            foreach (var mesTok in measureTokens)
            {
                var name = mesTok["name"].ToString();
                var exp = mesTok["expression"].Value<string>();
                var relativePath = Path.Combine(subFolderPath, $"{pathEscaper.EscapeName(name)}.dax");
                WriteToFile(Path.Combine(rootFolderPath, relativePath), exp);
                mesTok["expression"] = $"{{ref: {relativePath}}}";
            }
        }

        private void WriteToFile(string path, JToken obj)
        {
            WriteToFile(path, obj.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        private void WriteToFile(string path, string text)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, text);
        }
    }
}
