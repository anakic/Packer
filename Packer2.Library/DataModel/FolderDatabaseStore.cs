using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel
{
    class PartitionExpressionsMapping : JsonToFileMapping
    {
        protected override string ElementsSelector => ".partitions[*]";

        protected override string SubFolder => "Partitions";

        protected override string GetSubfolderForElement(JToken elem) => String.Empty;

        protected override IEnumerable<JsonToFileMapping> ChildMappings => Array.Empty<JsonToFileMapping>();

        static Dictionary<string, string> expTypeToExtensionsMap = new Dictionary<string, string>() { { "m", "m" }, { "calculated", "dax" } };
        protected override string GetFileName(JToken elem)
        {
            var name = elem["name"]!.Value<string>();
            var type = elem.SelectToken("source.type")!.Value<string>()!;
            var ext = expTypeToExtensionsMap[type];
            return $"{name}.{ext}";
        }

        public override string GetPayload(JToken obj)
            => (string)obj["source"]!["expression"]!;

        protected override void RegisterPayloadLocation(JToken elem, string destinationFileName)
        {
            elem.SelectToken("source.expression").Parent.Remove();
            elem["source"]!["expressionFileRef"] = destinationFileName;
        }

        protected override string ReadPayloadLocation(JToken elem)
            => (string)elem["source"]!["expressionFileRef"]!;

        protected override JToken ApplyElementPayload(JToken elem, string payload)
        {
            elem.SelectToken("source.expressionFileRef").Parent.Remove();
            elem["source"]!["expression"] = payload;
            return elem;
        }
    }

    class TablesMapping : JsonToFileMapping
    {
        protected override string ElementsSelector => ".model.tables[*]";

        protected override string SubFolder => "Tables";

        protected override string GetSubfolderForElement(JToken elem)
            => (string)elem["name"]!;

        protected override string GetFileName(JToken elem) => "table.json";

        protected override IEnumerable<JsonToFileMapping> ChildMappings { get; }

        protected override void RegisterPayloadLocation(JToken elem, string destinationFileName)
            => elem.Replace(JObject.FromObject(new { tableFileRef = destinationFileName, name = (string)elem["name"] }));

        protected override string ReadPayloadLocation(JToken elem)
            => (string)elem["tableFileRef"]!;

        protected override JToken ApplyElementPayload(JToken elem, string contents)
        {
            var elemNew = JObject.Parse(contents);
            elem.Replace(elemNew);
            return elemNew;
        }

        public TablesMapping()
        {
            ChildMappings = new JsonToFileMapping[]
            {
                new PartitionExpressionsMapping(),
                // column dax expressions
                // measure dax expressions
            };
        }
    }


    class RootMapping : JsonToFileMapping
    {
        protected override string SubFolder => string.Empty;

        protected override string ElementsSelector => "";

        protected override IEnumerable<JsonToFileMapping> ChildMappings { get; }

        protected override string GetFileName(JToken elem) => "database.json";
        protected override string GetSubfolderForElement(JToken elem) => string.Empty;
        protected override void RegisterPayloadLocation(JToken elem, string destinationFileName) { }
        protected override JToken ApplyElementPayload(JToken elem, string baseFolder) => elem;
        protected override string ReadPayloadLocation(JToken elem) => string.Empty;

        public RootMapping()
        {
            ChildMappings = new JsonToFileMapping[] 
            {
                new TablesMapping(),
                // new ExpressionsMapping(),
            };
        }

        public void Write(JToken token, string baseFolder)
            => Write(token, baseFolder, string.Empty);

        public JObject Read(string baseFolder)
        {
            JObject jObj = JObject.Parse(File.ReadAllText(Path.Combine(baseFolder, "database.json")));
            Read(jObj, baseFolder, string.Empty);
            return jObj;
        }
    }

    abstract class JsonToFileMapping
    {
        PathEscaper pathEscaper = new PathEscaper();

        // root folder for all items of this type
        protected abstract string SubFolder { get; }

        // how to find child elements in the parent jobj
        protected abstract string ElementsSelector { get; }

        // which child elements are we mapping (recursively)
        protected abstract IEnumerable<JsonToFileMapping> ChildMappings { get; }


        protected abstract string GetFileName(JToken elem);
        protected abstract string GetSubfolderForElement(JToken elem);

        public virtual string GetPayload(JToken obj)
            => obj.ToString(Newtonsoft.Json.Formatting.Indented);

        protected abstract void RegisterPayloadLocation(JToken elem, string destinationFileName);

        protected abstract JToken ApplyElementPayload(JToken elem, string baseFolder);

        protected abstract string ReadPayloadLocation(JToken elem);

        protected void Read(JToken obj, string baseFolder, string relativeFolder)
        {
            var elements = obj.SelectTokens(ElementsSelector);
            foreach (var elem in elements.ToArray())
            {
                var element = elem;

                // where shall we the data for this element from
                var subFolderForElement = Path.Combine(relativeFolder, SubFolder, pathEscaper.EscapeName(GetSubfolderForElement(elem)));

                // read the stored payload for this element
                var payloadLocation = ReadPayloadLocation(elem);
                
                // todo: replace this with mapping handling reading/writing
                if (!string.IsNullOrEmpty(payloadLocation))
                {
                    var payload = File.ReadAllText(Path.Combine(baseFolder, relativeFolder, payloadLocation));
                    // then apply the payload to the element
                    element = ApplyElementPayload(elem, payload);
                }
                // read child elements first
                foreach (var childMap in ChildMappings)
                    childMap.Read(element, baseFolder, subFolderForElement);
            }
        }

        protected void Write(JToken obj, string baseFolder, string relativeFolder)
        {
            var elements = obj.SelectTokens(ElementsSelector);
            foreach (var elem in elements.ToArray())
            {
                // where shall we store the data from this element
                var subFolderForElementRelativeToParent = Path.Combine(SubFolder, pathEscaper.EscapeName(GetSubfolderForElement(elem)));
                var subFolderForElementRelativeToBase = Path.Combine(relativeFolder, subFolderForElementRelativeToParent);
                
                // process child mappings first (while everything is in the same file)
                foreach (var childMap in ChildMappings)
                    childMap.Write(elem, baseFolder, subFolderForElementRelativeToBase);

                // then extract the payload from the JToken
                var payload = GetPayload(elem);

                // store the payload into a file
                var destinationFileNameRelativeToParent = Path.Combine(subFolderForElementRelativeToParent, pathEscaper.EscapeName(GetFileName(elem)));
                WriteToFile(Path.Combine(baseFolder, relativeFolder, destinationFileNameRelativeToParent), payload);

                // register the location of the payload in the JToken
                RegisterPayloadLocation(elem, destinationFileNameRelativeToParent);
            }
        }

        private void WriteToFile(string path, string text)
        {
            var dir = Path.GetDirectoryName(path)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, text);
        }
    }

    public class FolderDatabaseStore : FolderModelStore<Database>
    {
        PathEscaper pathEscaper = new PathEscaper();

        class JObjFile : ITextFile
        {
            public JObject? JObject { get; private set; }

            public JObjFile(JObject? jobj = null)
            {
                JObject = jobj;
            }

            public string GetText()
            {
                return JObject?.ToString() ?? "";
            }

            public void SetText(string text)
            {
                JObject = JObject.Parse(text);
            }
        }

        string folder;
        private readonly ILogger<FolderDatabaseStore> logger;

        public FolderDatabaseStore(string folder, ILogger<FolderDatabaseStore>? logger = null)
            : base(folder)
        {
            this.folder = folder;
            this.logger = logger ?? new DummyLogger<FolderDatabaseStore>();
        }

        public override Database Read()
        {
            // todo: use {ref:} (recursively) to resolve everything. Write shoud take care to add {ref:} elements where needed.

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

        protected override void DoSave(Database model)
        {
            var jObjFile = new JObjFile();
            var inner = new BimDataModelStore(jObjFile);
            inner.Save(model);

            var mapping = new RootMapping();
            mapping.Write(jObjFile.JObject!, folder);

            var x = mapping.Read(folder);

            var jobj = jObjFile.JObject!;
            ExpandTables(jobj, folder);
            ExpandExpressions(jobj, folder);
            WriteToFile(Path.Combine(folder, "database.json"), jobj);
        }

        private void ExpandExpressions(JObject jObj, string folderPath)
        {
            IEnumerable<JToken> nodes = jObj.SelectTokens(".model.expressions[*]");
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
                var tableFolderPath = Path.Combine(path, "Tables", pathEscaper.EscapeName(name));
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
