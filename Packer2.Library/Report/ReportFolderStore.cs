using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Packer2.Library;
using Packer2.Library.Tools;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace DataModelLoader.Report
{
    public class ReportFolderStore : FolderModelStore<PowerBIReport>
    {
        #region mapping to folder
        class ReportMappedFolder : JsonMappedFolder
        {
            protected override string RootFileName => "layout.json";

            protected override IEnumerable<MappingZone> Mappings { get; }

            public ReportMappedFolder()
            {
                Mappings = new MappingZone[]
                {
                    new PagesZone(),
                    new BookmarkZone(),
                    new ChildBookmarkZone()
                };
            }
        }

        class PagesZone : JsonElementZone
        {
            protected override string ContainingFolder => "Pages";

            protected override string ElementsSelector => ".sections[*]";

            protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

            protected override string GetFileName(JToken elem) => (string)elem["name"]!;

            protected override string GetFileExtension(JToken elem) => "json";

            // pages not expanded further (no child mappings so no need for subfolder, at least for now)
            protected override string GetSubfolderForElement(JToken elem) => String.Empty;
        }

        class BookmarkZone : JsonElementZone
        {
            protected override string ContainingFolder => "Bookmarks";

            protected override string ElementsSelector => ".#config.bookmarks[?(@.explorationState)]";

            protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

            protected override string GetFileExtension(JToken elem) => "json";

            protected override string GetFileName(JToken elem) => (string)elem["name"]!;

            protected override string GetSubfolderForElement(JToken elem) => String.Empty;
        }

        class ChildBookmarkZone : JsonElementZone
        {
            protected override string ContainingFolder => "Bookmarks";

            protected override string ElementsSelector => ".#config.bookmarks[*].children[*]";

            protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

            protected override string GetFileExtension(JToken elem) => "json";

            protected override string GetFileName(JToken elem) => (string)elem["name"]!;

            protected override string GetSubfolderForElement(JToken elem)
                => elem.Parent.Parent.Parent["displayName"].ToString();
        }
        #endregion

        #region transforms

        interface IJObjTransform
        {
            void Transform(JObject obj);
            void Restore(JObject obj);
        }

        class ConsolidateOrderingTransform : IJObjTransform
        {
            public void Restore(JObject obj)
            {
                foreach (var jo in obj.SelectTokens(".sections[*]").OfType<JObject>())
                {
                    RestoreConsolidatedProperty(jo, "tabOrder");
                    RestoreConsolidatedProperty(jo, "z");
                }
            }

            public void Transform(JObject obj)
            {
                foreach (var jo in obj.SelectTokens(".sections[*]").OfType<JObject>())
                {
                    Consolidate(jo, "tabOrder");
                    Consolidate(jo, "z");
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

            private static void RestoreConsolidatedProperty(JObject pageFileJObj, string property)
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
        }

        class ConfigStuffTransform : IJObjTransform
        {
            public void Restore(JObject obj)
            {
                obj["config"] = ((JObject)obj["#config"]!).ToString(Formatting.None);
            }

            public void Transform(JObject obj)
            {
                var configObj = obj.SelectToken(".config")!;
                var configObjParsed = JObject.Parse(configObj.Value<string>()!);
                configObj.Parent!.Replace(new JProperty("#config", configObjParsed));
            }
        }

        class StripVisualStatePropertiesTransform : IJObjTransform
        {
            public void Restore(JObject obj)
            {
            }

            public void Transform(JObject obj)
            {
                var propertiesToRemove = new string[] { "query", "dataTransforms" };
                foreach (var t in obj.SelectTokens(".sections[*].visualContainers[*]"))
                {
                    foreach (var prop in propertiesToRemove)
                    {
                        var tok = t.SelectToken(prop);
                        if (tok != null)
                            tok.Parent!.Remove();
                    }
                }
            }
        }
        #endregion

        private readonly string folderPath;
        private readonly ILogger<ReportFolderStore> logger;
        ReportMappedFolder reportFolderMapper = new ReportMappedFolder();
        List<IJObjTransform> transforms;

        public ReportFolderStore(string folderPath, ILogger<ReportFolderStore>? logger = null)
            : base(folderPath)
        {
            transforms = new List<IJObjTransform>
            {
                new ConfigStuffTransform(),
                new ConsolidateOrderingTransform(), 
                new StripVisualStatePropertiesTransform() 
            };

            this.folderPath = folderPath;
            this.logger = logger ?? new DummyLogger<ReportFolderStore>();
        }

        public override PowerBIReport Read()
        {
            var model = new PowerBIReport();
            var blobFolderPath = Path.Combine(folderPath, "Blobs");
            foreach (var file in Directory.GetFiles(blobFolderPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = PathTools.GetRelativePath(file, blobFolderPath);
                // todo: log
                model.Blobs[relativePath] = File.ReadAllBytes(file);
            }

            // todo: log
            model.Connections = ReadJsonFile(Path.Combine(folderPath, "Connections.json"));
            model.Content_Types = ReadXmlFile(Path.Combine(folderPath, "[Content_Types].xml"));
            model.DataModelSchemaFile = ReadJsonFile(Path.Combine(folderPath, "DataModelSchema.json"));
            model.DiagramLayout = ReadJsonFile(Path.Combine(folderPath, "DiagramLayout.json"));
            model.Metadata = ReadJsonFile(Path.Combine(folderPath, "Metadata.json"));
            model.Settings = ReadJsonFile(Path.Combine(folderPath, "Settings.json"));
            model.Version = File.ReadAllText(Path.Combine(folderPath, "Version.txt"));
            model.Report_LinguisticSchema = ReadXmlFile(Path.Combine(folderPath, "Report\\LinguisticSchema.xml"));

            var rpt = reportFolderMapper.Read(Path.Combine(folderPath, "Report"));
            transforms.ForEach(t => t.Restore(rpt));
            model.Layout = rpt;

            return model;
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

        protected override void DoSave(PowerBIReport model)
        {
            foreach (var kvp in model.Blobs)
            {
                var path = Path.Combine(folderPath, "Blobs", kvp.Key);
                FileTools.WriteToFile(path, kvp.Value);
            }

            // todo: define or reuse constants for file names
            FileTools.WriteToFile(Path.Combine(folderPath, "Connections.json"), model.Connections?.ToString(Formatting.Indented));
            FileTools.WriteToFile(Path.Combine(folderPath, "[Content_Types].xml"), model.Content_Types.ToString());
            FileTools.WriteToFile(Path.Combine(folderPath, "DataModelSchema.json"), model.DataModelSchemaFile?.ToString(Formatting.Indented));
            FileTools.WriteToFile(Path.Combine(folderPath, "DiagramLayout.json"), model.DiagramLayout.ToString(Formatting.Indented));
            FileTools.WriteToFile(Path.Combine(folderPath, "Metadata.json"), model.Metadata.ToString(Formatting.Indented));
            FileTools.WriteToFile(Path.Combine(folderPath, "Settings.json"), model.Settings.ToString(Formatting.Indented));
            FileTools.WriteToFile(Path.Combine(folderPath, "Version.txt"), model.Version);
            FileTools.WriteToFile(Path.Combine(folderPath, "Report\\LinguisticSchema.xml"), model.Report_LinguisticSchema?.ToString());

            // we're mutating the JObject so working on a copy just to do things by the book. using the original
            // object would probably not cause any issues because nobody else is using it, but there's no guarantee
            // this will always continue to be the case so using the clone just in case.
            var layoutJObjClone = (JObject)model.Layout.DeepClone();
            transforms.ForEach(t => t.Transform(layoutJObjClone));
            reportFolderMapper.Write(layoutJObjClone, Path.Combine(folderPath, "Report"));
        }
    }
}
