using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Packer2.Library;
using Packer2.Library.Tools;
using System.Xml.Linq;

namespace DataModelLoader.Report
{
    public class ReportFolderStore : IModelStore<PowerBIReport>
    {
        PathEscaper pathEscaper = new PathEscaper();

        private readonly string folderPath;

        public ReportFolderStore(string folderPath, ILogger<ReportFolderStore> logger = null)
        {
            this.folderPath = folderPath;
        }

        public PowerBIReport Read()
        {
            var model = new PowerBIReport();
            var blobFolderPath = Path.Combine(folderPath, "Blobs");
            foreach (var file in Directory.GetFiles(blobFolderPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = PathTools.GetRelativePath(file, blobFolderPath);
                model.Blobs[relativePath] = File.ReadAllBytes(file);
            }

            model.Connections = ReadJsonFile(Path.Combine(folderPath, "Connections.json"));
            model.Content_Types = ReadXmlFile(Path.Combine(folderPath, "[Content_Types].xml"));
            model.DataModelSchemaFile = ReadJsonFile(Path.Combine(folderPath, "DataModelSchema.json"));
            model.DiagramLayout = ReadJsonFile(Path.Combine(folderPath, "DiagramLayout.json"));
            model.Metadata = ReadJsonFile(Path.Combine(folderPath, "Metadata.json"));
            model.Settings = ReadJsonFile(Path.Combine(folderPath, "Settings.json"));
            model.Version = File.ReadAllText(Path.Combine(folderPath, "Version.txt"));
            model.Report_LinguisticSchema = ReadXmlFile(Path.Combine(folderPath, "Report\\LinguisticSchema.xml"));

            var layoutJObj = ReadJsonFile(Path.Combine(folderPath, "Report\\Layout.json"));
            var layoutConfigJObj = ReadJsonFile(Path.Combine(folderPath, "Report\\config.json"));

            var bookmarksDir = Path.Combine(folderPath, "Report\\Bookmarks");
            if (Directory.Exists(bookmarksDir))
            {
                foreach (var bf in Directory.GetFiles(bookmarksDir))
                {
                    var bookmarkJObj = ReadJsonFile(bf);
                    var groupVal = bookmarkJObj["$GroupName"];
                    if (groupVal != null)
                    {
                        (layoutConfigJObj.SelectToken($".bookmarks[?(@.displayName=='{groupVal.Value<string>()}')].children") as JArray)!.Add(bookmarkJObj);
                    }
                    else
                        (layoutConfigJObj.SelectToken($".bookmarks") as JArray)!.Add(bookmarkJObj);
                }
            }
            layoutJObj["config"] = layoutConfigJObj.ToString(Formatting.None);

            var pagesArr = layoutJObj["sections"] as JArray;
            foreach (var pf in Directory.GetFiles(Path.Combine(folderPath, @"Report\Pages")))
            {
                var pageJObj = ReadJsonFile(pf);
                Restore(pageJObj, "tabOrder");
                Restore(pageJObj, "z");
                pagesArr!.Add(pageJObj);
            }
            model.Layout = layoutJObj;

            return model;
        }

        private static void Restore(JObject pageFileJObj, string property)
        {
            var arrToken = (pageFileJObj.SelectToken($"#{property}") as JArray)!;
            var tabOrderArr = arrToken.Values<string>()!.ToArray();

            var visualConfigs = pageFileJObj
                .SelectTokens("visualContainers[*]")!
                .ToDictionary(tok => tok!, tok => JObject.Parse(tok["config"]!.ToString()));

            var dict = visualConfigs
                .ToDictionary(kvp => kvp.Value["name"]!.ToString(), kvp => (JObject)kvp.Key);

            int order = 1;
            foreach (var visualName in tabOrderArr)
            {
                var value = 100 * order++;
                var visualToken = dict[visualName!];
                visualToken.Add(new JProperty(property, value));
                var configObj = JObject.Parse(visualToken["config"]!.ToString());
                (configObj.SelectToken($".layouts[0].position") as JObject)!.Add(new JProperty(property, value));
                visualToken["config"] = configObj.ToString(Formatting.None);
            }
            ((JProperty)arrToken.Parent!).Remove();
        }

        private JObject? ReadJsonFile(string path)
        {
            if (File.Exists(path) == false)
                return null;
            return JObject.Parse(File.ReadAllText(path));
        }
        private XDocument? ReadXmlFile(string path)
        {
            if (File.Exists(path) == false)
                return null;
            return XDocument.Parse(File.ReadAllText(path));
        }

        public void Save(PowerBIReport model)
        {
            ClearFolder();

            foreach (var kvp in model.Blobs)
            {
                var path = Path.Combine(folderPath, "Blobs", kvp.Key);
                EnsureDirectoryExists(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, kvp.Value);
            }

            // todo: define or reuse constants for file names
            SaveFile(Path.Combine(folderPath, "Connections.json"), model.Connections?.ToString(Formatting.Indented));
            SaveFile(Path.Combine(folderPath, "[Content_Types].xml"), model.Content_Types.ToString());
            SaveFile(Path.Combine(folderPath, "DataModelSchema.json"), model.DataModelSchemaFile?.ToString(Formatting.Indented));
            SaveFile(Path.Combine(folderPath, "DiagramLayout.json"), model.DiagramLayout.ToString(Formatting.Indented));
            SaveFile(Path.Combine(folderPath, "Metadata.json"), model.Metadata.ToString(Formatting.Indented));
            SaveFile(Path.Combine(folderPath, "Settings.json"), model.Settings.ToString(Formatting.Indented));
            SaveFile(Path.Combine(folderPath, "Version.txt"), model.Version);
            SaveFile(Path.Combine(folderPath, "Report\\LinguisticSchema.xml"), model.Report_LinguisticSchema?.ToString());

            // we're mutating the JObject so working on a copy just to do things by the book. using the original
            // object would probably not cause any issues because nobody else is using it, but there's no guarantee
            // this will always continue to be the case so using the clone just in case.
            var layoutJObjClone = (JObject)model.Layout.DeepClone();
            StripVisualStateProperties(layoutJObjClone);
            SaveLayoutConfig(layoutJObjClone);
            SavePages(layoutJObjClone);
            SaveFile(Path.Combine(folderPath, @"Report\Layout.json"), layoutJObjClone.ToString(Formatting.Indented));
        }

        private void ClearFolder()
        {
            if (Directory.Exists(folderPath))
            {
                foreach (var childDir in Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly))
                {
                    // do not remove the .git folder
                    if (Path.GetFileName(childDir) != ".git")
                        Directory.Delete(childDir, true);
                }

                foreach (var file in Directory.GetFiles(folderPath))
                    File.Delete(file);
            }
        }

        private void StripVisualStateProperties(JObject layoutJObjClone)
        {
            var propertiesToRemove = new string[] { "query", "dataTransforms" };
            foreach (var t in layoutJObjClone.SelectTokens(".sections[*].visualContainers[*]"))
            {
                foreach (var prop in propertiesToRemove)
                {
                    var tok = t.SelectToken(prop);
                    if (tok != null)
                        tok.Parent!.Remove();
                }
            }
        }

        private void SavePages(JObject layout)
        {
            foreach (JObject pageFileObj in layout.SelectTokens(".sections[*]").ToArray())
            {
                Consolidate(pageFileObj, "tabOrder");
                Consolidate(pageFileObj, "z");

                var pageName = pageFileObj["displayName"]!.ToString();
                SaveFile(Path.Combine(folderPath, @"Report\Pages", $"{pathEscaper.EscapeName(pageName)}.json"), pageFileObj.ToString(Formatting.Indented));
                pageFileObj.Remove();
            }
        }

        private static void Consolidate(JObject pageFileJObj, string property)
        {
            Dictionary<string, int> visualsOrder = new Dictionary<string, int>();
            foreach (var container in pageFileJObj.SelectTokens("visualContainers[*]"))
            {
                var toToken = container[property];
                if (toToken == null)
                    continue;

                var value = toToken.Value<int>();
                container.SelectToken(property)!.Parent!.Remove();

                var configObj = JObject.Parse(container["config"]!.ToString());
                configObj.SelectToken($".layouts[0].position.{property}")!.Parent!.Remove();
                container["config"] = configObj.ToString(Formatting.None);
                var visualName = configObj["name"]!.Value<string>()!;
                visualsOrder[visualName] = value;
            }
            var visualsOrderArr = visualsOrder.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray();
            pageFileJObj.Add(new JProperty($"#{property}", visualsOrderArr));
        }

        private void SaveLayoutConfig(JObject layoutFile)
        {
            var configJValue = layoutFile.SelectToken(".config");
            var config = configJValue!.Value<string>()!;

            JObject configObj = JObject.Parse(config);
            List<JObject> bookmarkJObjs = new List<JObject>();
            foreach (JObject bookmarkObj in configObj.SelectTokens(".bookmarks[*]"))
            {
                var children = bookmarkObj.SelectToken("children") as JArray;
                if (children != null)
                {
                    foreach (JObject child in children)
                    {
                        child["$GroupName"] = bookmarkObj["displayName"]!.Value<string>();
                        bookmarkJObjs.Add(child);
                    }
                }
                else
                    bookmarkJObjs.Add(bookmarkObj);
            }

            foreach (var pageJObj in bookmarkJObjs)
            {
                var pageName = pageJObj["displayName"]!.Value<string>()!;
                SaveFile(Path.Combine(folderPath, @"Report\Bookmarks", $"{pathEscaper.EscapeName(pageName)}.json"), pageJObj.ToString(Formatting.Indented));
                pageJObj.Remove();
            }
            SaveFile(Path.Combine(folderPath, @"Report\Config.json"), configObj.ToString(Formatting.Indented));
            configJValue!.Parent!.Remove();
        }

        private void SaveFile(string path, string? text)
        {
            if (text != null)
            {
                EnsureDirectoryExists(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, text);
            }
        }

        private void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }
}
