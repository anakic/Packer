﻿using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel
{
    public class FolderModelStore : IDataModelStore
    {
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
            WriteToFile(Path.Combine(folder, "database.json"), jobj);
        }

        private void ExpandTables(JObject jobj, string path)
        {
            foreach (var tok in jobj.SelectTokens(".model.tables[*]").ToArray())
            {
                string name = tok["name"].Value<string>();
                var tableFolderPath = Path.Combine(path, EscapeName(name));
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
                var relativePath = Path.Combine(subFolderPath, $"{EscapeName(name)}.{ext}");
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
                var relativePath = Path.Combine(subFolderPath, $"{EscapeName(name)}.dax");
                WriteToFile(Path.Combine(rootFolderPath, relativePath), exp);
                mesTok["expression"] = $"{{ref: {relativePath}}}";
            }
        }

        private string EscapeName(string name)
            => new Uri("file:///" + name).AbsolutePath.Substring(1);

        // don't need this currently, all the names we need are still in the extracted json files
        //private string UnescapeName(string name)
        //    => new Uri("file:///" + name).LocalPath.Substring(1);

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
