using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Packer2.FileSystem;
using Packer2.Library;
using Packer2.Library.Tools;
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
            IEnumerable<MappingZone> childMappings;
            public PagesZone()
            {
                childMappings = new List<MappingZone>() { new VisualZone() };
            }

            protected override string ContainingFolder => "Pages";

            protected override string ElementsSelector => ".sections[*]";

            protected override IEnumerable<MappingZone> ChildMappings => childMappings;

            protected override string GetFileName(JToken elem) => "page";

            protected override string GetFileExtension(JToken elem) => "json";

            protected override string GetSubfolderForElement(JToken elem) => (string)elem["displayName"]!;
        }

        class BookmarkZone : JsonElementZone
        {
            protected override string ContainingFolder => "Bookmarks";

            // how to find bookmarks in original json
            protected override string ElementsSelector => ".#config.bookmarks[*]";

            // only target bookmarks that do not have children
            protected override bool FilterSelectedElements(JToken element)
                => element["children"] == null;

            protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

            protected override string GetFileExtension(JToken elem) => "json";

            protected override string GetFileName(JToken elem) => (string)elem["displayName"]!;

            protected override string GetSubfolderForElement(JToken elem) => String.Empty;
        }

        class ChildBookmarkZone : JsonElementZone
        {
            protected override string ContainingFolder => "Bookmarks";

            protected override string ElementsSelector => ".#config.bookmarks[*].children[*]";

            protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

            protected override string GetFileExtension(JToken elem) => "json";

            protected override string GetFileName(JToken elem) => (string)elem["displayName"]!;

            protected override string GetSubfolderForElement(JToken elem)
                => elem.Parent.Parent.Parent["displayName"].ToString();
        }

        class VisualZone : JsonElementZone
        {
            protected override string ContainingFolder => string.Empty;

            protected override string ElementsSelector => ".visualContainers[*].#config";

            protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

            protected override string GetFileExtension(JToken elem) => "json";

            protected override string GetFileName(JToken elem) 
            {
                if (elem["singleVisualGroup"] != null)
                    return elem.SelectToken("singleVisualGroup.displayName")!.ToString();
                else
                    return elem.SelectToken("name")!.ToString();
            }

            protected override string GetSubfolderForElement(JToken elem)
            {
                if (elem["singleVisualGroup"] != null)
                    return "VisualGroups";
                else
                    return elem.SelectToken("singleVisual.visualType")!.ToString();
            }
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
                var expandedConfigObj = obj.SelectToken(".#config")!;
                expandedConfigObj.Parent!.Replace(new JProperty("config", ((JObject)obj["#config"]!).ToString(Formatting.None)));
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

        private const string ConnectionsFilePath = "Connections.json";
        private const string ContentTypesFilePath = "[Content_Types].xml";
        private const string DataModelSchemaFilePath = "DataModelSchema.json";
        private const string DiagramLayoutFilePath = "DiagramLayout.json";
        private const string MedataFilePath = "Metadata.json";
        private const string SettingsFilePath = "Settings.json";
        private const string VersionFilePath = "Version.txt";
        private const string ReportLinguisticSchemaFilePath = "Report\\LinguisticSchema.xml";
        private const string ReportFolderPath = "Report";

        private readonly IFileSystem fileSystem;
        private readonly ILogger<ReportFolderStore> logger;
        ReportMappedFolder reportFolderMapper = new ReportMappedFolder();
        List<IJObjTransform> transforms;

        public ReportFolderStore(string folderPath, ILogger<ReportFolderStore>? logger = null)
            : this(new LocalFileSystem(folderPath), logger)
        {
        }

        public ReportFolderStore(IFileSystem fileSystem, ILogger<ReportFolderStore>? logger = null)
            : base(fileSystem)
        {
            transforms = new List<IJObjTransform>
            {
                new ConfigStuffTransform(),
                new ConsolidateOrderingTransform(), 
                new StripVisualStatePropertiesTransform() 
            };

            this.fileSystem = fileSystem;
            this.logger = logger ?? new DummyLogger<ReportFolderStore>();
        }

        public override PowerBIReport Read()
        {
            var model = new PowerBIReport();
            
            // todo: introduce constant for "Blobs"
            foreach (var file in fileSystem.GetFilesRecursive("Blobs"))
            {
                logger.LogInformation("Reading blob file '{filePath}'.", file);
                var blobFileName = fileSystem.PathResolver.GetRelativePath(file, "Blobs");
                model.Blobs[blobFileName] = fileSystem.ReadAsBytes(file)!;
            }

            model.Connections = ReadJsonFile(ConnectionsFilePath);
            model.Content_Types = ReadXmlFile(ContentTypesFilePath);
            model.DataModelSchemaFile = ReadJsonFile(DataModelSchemaFilePath);
            model.DiagramLayout = ReadJsonFile(DiagramLayoutFilePath);
            model.Metadata = ReadJsonFile(MedataFilePath);
            model.Settings = ReadJsonFile(SettingsFilePath);
            model.Version = ReadTextFile(VersionFilePath);
            model.Report_LinguisticSchema = ReadXmlFile(ReportLinguisticSchemaFilePath);

            var rpt = reportFolderMapper.Read(fileSystem.Sub(ReportFolderPath));
            transforms.ForEach(t => 
            { 
                logger.LogInformation("Restoring report transformation '{transformation}'", t.GetType().Name); 
                t.Restore(rpt); 
            });
            model.Layout = rpt;

            return model;
        }

        protected override void DoSave(PowerBIReport model)
        {
            foreach (var kvp in model.Blobs)
            {
                var path = Path.Combine("Blobs", kvp.Key);
                logger.LogInformation("Writing blob file '{filePath}'.", path);
                fileSystem.Save(path, kvp.Value);
            }

            // todo: define or reuse constants for file names
            fileSystem.Save(ConnectionsFilePath, model.Connections?.ToString(Formatting.Indented));
            fileSystem.Save(ContentTypesFilePath, model.Content_Types.ToString());
            fileSystem.Save(DataModelSchemaFilePath, model.DataModelSchemaFile?.ToString(Formatting.Indented));
            fileSystem.Save(DiagramLayoutFilePath, model.DiagramLayout.ToString(Formatting.Indented));
            fileSystem.Save(MedataFilePath, model.Metadata.ToString(Formatting.Indented));
            fileSystem.Save(SettingsFilePath, model.Settings.ToString(Formatting.Indented));
            fileSystem.Save(VersionFilePath, model.Version);
            fileSystem.Save(ReportLinguisticSchemaFilePath, model.Report_LinguisticSchema?.ToString());

            // we're mutating the JObject so working on a copy just to do things by the book. using the original
            // object would probably not cause any issues because nobody else is using it, but there's no guarantee
            // this will always continue to be the case so using the clone just in case.
            var layoutJObjClone = (JObject)model.Layout.DeepClone();
            transforms.ForEach(t =>
            {
                logger.LogInformation("Applying report transformation '{transformation}'", t.GetType().Name); 
                t.Transform(layoutJObjClone);
            });
            reportFolderMapper.Write(layoutJObjClone, fileSystem.Sub(ReportFolderPath));
        }

        private string? ReadTextFile(string path)
        {
            if (fileSystem.FileExists(path) == false)
            {
                logger.LogInformation("Attempted to read json file '{filePath}' but file does not exist.", path);
                return null;
            }

            logger.LogInformation("Reading text file '{filePath}'", path);
            return fileSystem.ReadAsString(path);
        }

        // todo: use ReadTextFile internally?
        private JObject? ReadJsonFile(string path)
        {
            if (fileSystem.FileExists(path) == false)
            {
                logger.LogInformation("Attempted to read json file '{filePath}' but file does not exist.", path);
                return null;
            }

            logger.LogInformation("Reading json file '{filePath}'", path);
            var content = fileSystem.ReadAsString(path);
            if (string.IsNullOrEmpty(content))
            {
                logger.LogInformation("Attempted to read json file '{filePath}' but file is empty.", path);
                return null;
            }
            return JObject.Parse(content);
        }

        // todo: use ReadTextFile internally?
        private XDocument? ReadXmlFile(string path)
        {
            if (fileSystem.FileExists(path) == false)
            {
                logger.LogInformation("Attempted to read xml file '{filePath}' but file does not exist.", path);
                return null;
            }

            logger.LogInformation("Reading xml file '{filePath}'", path);
            var content = fileSystem.ReadAsString(path);
            if (string.IsNullOrEmpty(content))
            {
                logger.LogInformation("Attempted to read json file '{filePath}' but file is empty.", path);
                return null;
            }
            return XDocument.Parse(content);
        }
    }
}
