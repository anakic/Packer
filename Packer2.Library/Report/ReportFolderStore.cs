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
                    return $"{elem.SelectToken("singleVisualGroup.displayName")!.ToString()} ({elem.SelectToken("name")!.ToString()})";
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

        class SimplifyBookmarks : IJObjTransform
        {
            public void Restore(JObject obj)
            {
                // restore nothing - that useless fluff is toast!
            }

            public void Transform(JObject obj)
            {
                List<JObject> bookmarkJobjs = new List<JObject>();
                foreach (var jo in obj.SelectTokens(".#config.bookmarks[*]").OfType<JObject>())
                {
                    var children = jo["children"] as JArray;
                    if (children == null)
                        bookmarkJobjs.Add(jo);
                    else
                        bookmarkJobjs.AddRange(children.OfType<JObject>());
                }

                foreach (var bookmarkJObj in bookmarkJobjs)
                {
                    var applyOnlyToTargetVisuals = bookmarkJObj.SelectToken("options.applyOnlyToTargetVisuals")?.Value<bool>() ?? false;

                    var targetVisualsArr = (JArray)bookmarkJObj.SelectToken("options.targetVisualNames")!;
                    var targetVisualNames = targetVisualsArr.ToObject<string[]>()!.ToHashSet();
                    var targetVisualNamesFound = new HashSet<string>();
                    var containers1 = ((JObject)bookmarkJObj.SelectToken("explorationState.sections..visualContainers")!).Properties();
                    var containers2 = (bookmarkJObj.SelectTokens("explorationState.sections..visualContainerGroups..children")!).SelectMany(t => ((JObject)t).Properties());

                    foreach (JProperty c in containers1.Union(containers2).ToList())
                    {
                        bool removed = false;

                        // remove the visual's node if not in targetVisuals and applyOnlyToTargetVisuals=true
                        if (applyOnlyToTargetVisuals && !targetVisualNames.Contains(c.Name))
                        {
                            c.Remove();
                            removed = true;
                        }

                        if (!removed)
                        {
                            // remove the visual's node if no useful data inside it
                            var singleVisualNode = (JObject)c.Value["singleVisual"]!;
                            if (singleVisualNode != null)
                            {
                                if (singleVisualNode.Properties().Count(p => new[] { "visualType", "objects" }.Contains(p.Name) == false) == 0)
                                {
                                    var objectsSubNode = (JObject)singleVisualNode["objects"]!;
                                    if (objectsSubNode == null || objectsSubNode.Properties().Count() == 0)
                                    {
                                        c.Remove();
                                        removed = true;
                                    }
                                }
                            }
                        }

                        if (!removed)
                            targetVisualNamesFound.Add(c.Name);
                    }

                    targetVisualsArr.Replace(new JArray(targetVisualNamesFound));

                    var suppressData = bookmarkJObj.SelectToken("options.suppressData")?.Value<bool>() ?? false;
                    if (suppressData)
                    {
                        var nodesToRemove1 = bookmarkJObj.SelectTokens("explorationState..filters").ToList();
                        var nodesToRemove2 = bookmarkJObj.SelectTokens("explorationState..visualContainers..singleVisual.activeProjections").ToList();
                        var nodesToRemove3 = bookmarkJObj.SelectTokens("explorationState..visualContainers..singleVisual.orderBy").ToList();
                        nodesToRemove1.Union(nodesToRemove2).Union(nodesToRemove3).ToList().ForEach(x => x.Parent.Remove());
                    }
                }
            }
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

                    var configObj = (JObject)container["#config"]!;
                    configObj.SelectToken($".layouts[0].position.{property}")!.Parent!.Remove();
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

                var dict = pageFileJObj.SelectTokens("visualContainers[*]")!
                    .ToDictionary(vc => vc.SelectToken("#config.name")!.ToString(), kvp => (JObject)kvp);

                int order = 1;
                foreach (var visualName in tabOrderArr)
                {
                    var value = 100 * order++;
                    var visualToken = dict[visualName!];
                    visualToken.Add(new JProperty(property, value));
                    var configObj = (JObject)visualToken["#config"]!;
                    (configObj.SelectToken($".layouts[0].position") as JObject)!.Add(new JProperty(property, value));
                }
                ((JProperty)arrToken.Parent!).Remove();
            }
        }

        class UnstuffTransform : IJObjTransform
        {
            public void Restore(JObject obj)
            {
                foreach (var configJObj in obj.SelectTokens("..#config").ToArray())
                    configJObj.Parent!.Replace(new JProperty("config", configJObj.ToString(Formatting.None)));

                foreach (var filtersJArr in obj.SelectTokens("..#filters").ToArray())
                    filtersJArr.Parent!.Replace(new JProperty("filters", filtersJArr.ToString(Formatting.None)));
            }

            public void Transform(JObject obj)
            {
                var tokens = obj.SelectTokens("..config").Where(t => t?.Type == JTokenType.String).ToArray();
                foreach (var tok in tokens)
                {
                    var jObj = JObject.Parse(tok.Value<string>()!);
                    tok.Parent!.Replace(new JProperty("#config", jObj));
                }

                var filtersTokens = obj.SelectTokens("..filters").Where(t => t?.Type == JTokenType.String).ToArray();
                foreach (var tok in filtersTokens)
                {
                    var jArr = JArray.Parse(tok.Value<string>()!).OfType<JObject>().ToArray();
                    tok.Parent!.Replace(new JProperty("#filters", jArr));
                }
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
                new UnstuffTransform(),
                new ConsolidateOrderingTransform(),
                new StripVisualStatePropertiesTransform(),
                new SimplifyBookmarks()
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
            transforms.Reverse<IJObjTransform>().ToList().ForEach(t =>
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
