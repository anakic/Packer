using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel
{
    class PartitionExpressionsMapping : JsonPropertyZone
    {
        protected override string ElementsSelector => ".partitions[*]";

        protected override string ContainingFolder => "Partitions";

        protected override string PayloadContainingObjectJsonPath => ".source";

        protected override string PayloadProperty => "expression";


        static Dictionary<string, string> expTypeToExtensionsMap = new Dictionary<string, string>() { { "m", "m" }, { "calculated", "dax" } };
        protected override string GetFileName(JToken elem)
        {
            var name = elem["name"]!.Value<string>();
            var type = elem.SelectToken("source.type")!.Value<string>()!;
            var ext = expTypeToExtensionsMap[type];
            return $"{name}.{ext}";
        }
    }

    class ColumnExpressionsMapping : JsonPropertyZone
    {
        protected override string PayloadContainingObjectJsonPath => "";

        protected override string PayloadProperty => "expression";

        protected override string ContainingFolder => "Columns";

        protected override string ElementsSelector => ".columns[?(@.type=='calculated')]";

        protected override string GetFileName(JToken elem) => $"{(string)elem["name"]!}.dax";
    }

    class MeasureExpressionsMapping : JsonPropertyZone
    {
        protected override string PayloadContainingObjectJsonPath => "";

        protected override string PayloadProperty => "expression";

        protected override string ContainingFolder => "Measures";

        protected override string ElementsSelector => ".measures[*]";

        protected override string GetFileName(JToken elem) => $"{(string)elem["name"]!}.dax";
    }

    class ModelExpressionsMapping : JsonPropertyZone
    {
        protected override string PayloadContainingObjectJsonPath => "";

        protected override string PayloadProperty => "expression";

        protected override string ContainingFolder => "MQueries";

        protected override string ElementsSelector => ".model.expressions[*]";

        protected override string GetFileName(JToken elem) => (string)elem["name"]!;
    }

    class TablesMapping : JsonElementZone
    {
        protected override string ElementsSelector => ".model.tables[*]";

        protected override string ContainingFolder => "Tables";

        protected override string GetSubfolderForElement(JToken elem)
            => (string)elem["name"]!;

        protected override string GetFileName(JToken elem) => "table.json";

        protected override IEnumerable<MappingZone> ChildMappings { get; }

        public TablesMapping()
        {
            ChildMappings = new MappingZone[]
            {
                new PartitionExpressionsMapping(),
                new ColumnExpressionsMapping(),
                new MeasureExpressionsMapping()
            };
        }
    }

    class BimMappedRepository : JsonMappedRepository
    {
        public BimMappedRepository()
        {
            Mappings = new MappingZone[]
            {
                new TablesMapping(),
                new ModelExpressionsMapping()
            };
        }

        protected override IEnumerable<MappingZone> Mappings { get; }

        protected override string RootFileName => "database.json";
    }

    abstract class JsonMappedRepository
    {
        protected abstract string RootFileName { get; }

        protected abstract IEnumerable<MappingZone> Mappings { get; }

        protected string GetFilePath(string baseFolder) => Path.Combine(baseFolder, RootFileName);

        public void Write(JObject obj, string baseFolder)
        {
            foreach (var mapping in Mappings)
                mapping.Write(obj, baseFolder, string.Empty);
            var rootFilePath = GetFilePath(baseFolder);
            FileTools.WriteToFile(rootFilePath, obj);
        }

        public JObject Read(string baseFolder)
        {
            JObject jObj = JObject.Parse(File.ReadAllText(GetFilePath(baseFolder)));
            foreach (var map in Mappings)
                map.Read(jObj, baseFolder, string.Empty);
            return jObj;
        }
    }

    abstract class JsonPropertyZone : MappingZone
    {
        const string refPrefix = "fileRef-";

        protected sealed override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

        protected abstract string PayloadContainingObjectJsonPath { get; }

        protected abstract string PayloadProperty { get; }

        public sealed override string GetPayload(JToken obj)
            => (string)obj.SelectToken(PayloadContainingObjectJsonPath)![PayloadProperty]!;

        protected sealed override JToken ApplyElementPayload(JToken elem, string payload)
        {
            (elem.SelectToken(PayloadContainingObjectJsonPath)![refPrefix + PayloadProperty]!.Parent as JProperty)!.Replace(new JProperty(PayloadProperty, payload));
            return elem;
        }

        protected sealed override string ReadPayloadLocation(JToken elem)
        {
            return (string)elem.SelectToken(PayloadContainingObjectJsonPath)![refPrefix + PayloadProperty]!;
        }

        protected sealed override void RegisterPayloadLocation(JToken elem, string destinationFileName)
        {
            (elem.SelectToken(PayloadContainingObjectJsonPath)![PayloadProperty]!.Parent as JProperty)!.Replace(new JProperty(refPrefix + PayloadProperty, destinationFileName));
        }

        // JsonPropertyZone zones do not have children so they won't create new subfolders
        protected override string GetSubfolderForElement(JToken elem) => String.Empty;
    }

    abstract class JsonElementZone : MappingZone
    {
        const string RefProp = "fileRef";

        public sealed override string GetPayload(JToken obj) 
            => obj.ToString(Newtonsoft.Json.Formatting.Indented);

        protected sealed override JToken ApplyElementPayload(JToken elem, string payload)
        {
            var obj = JObject.Parse(payload);
            elem.Replace(obj);
            return obj;
        }

        protected sealed override string ReadPayloadLocation(JToken elem) 
            => (string)elem[RefProp]!;

        protected sealed override void RegisterPayloadLocation(JToken elem, string destinationPath) 
            =>  elem.Replace(new JObject(new JProperty(RefProp, destinationPath)));
    }

    abstract class MappingZone
    {
        PathEscaper pathEscaper = new PathEscaper();

        // root folder for all items of this type
        protected abstract string ContainingFolder { get; }

        // how to find child elements in the parent jobj
        protected abstract string ElementsSelector { get; }

        // which child elements are we mapping (recursively)
        protected abstract IEnumerable<MappingZone> ChildMappings { get; }
        
        // how shall we name a sub-sub-folder for each element (the complete relative path is [ZoneFolder]/[ElementFolder], e.g. Tables/Employees)
        protected abstract string GetSubfolderForElement(JToken elem);

        // how shall we name the file for an element
        protected abstract string GetFileName(JToken elem);

        // what payload should we store from the element (JToken) into the file
        public abstract string GetPayload(JToken obj);

        // change the element so that it only has the payload location
        protected abstract void RegisterPayloadLocation(JToken elem, string destinationFileName);

        protected abstract JToken ApplyElementPayload(JToken elem, string payload);

        protected abstract string ReadPayloadLocation(JToken elem);

        public void Read(JToken obj, string baseFolder, string relativeFolder)
        {
            var elements = obj.SelectTokens(ElementsSelector);
            foreach (var elem in elements.ToArray())
            {
                var element = elem;

                // read the stored payload for this element
                var payloadLocation = ReadPayloadLocation(elem);
                
                // read 
                var payload = File.ReadAllText(Path.Combine(baseFolder, relativeFolder, payloadLocation));

                // then apply the payload to the element (might replace the element, which is why it return an element)
                element = ApplyElementPayload(elem, payload);

                // which folder does this element store data in (so we can pass this to child zones)
                var subFolderForElement = Path.Combine(relativeFolder, ContainingFolder, pathEscaper.EscapeName(GetSubfolderForElement(element)));

                // read child elements first
                foreach (var childMap in ChildMappings)
                    childMap.Read(element, baseFolder, subFolderForElement);
            }
        }

        public void Write(JToken obj, string baseFolder, string relativeFolder)
        {
            var elements = obj.SelectTokens(ElementsSelector);
            foreach (var elem in elements.ToArray())
            {
                // where shall we store the data from this element
                var subFolderForElementRelativeToParent = Path.Combine(ContainingFolder, pathEscaper.EscapeName(GetSubfolderForElement(elem)));
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
        BimMappedRepository map = new BimMappedRepository();

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
            var jobject = map.Read(folder);
            var jObjFile = new JObjFile(jobject);
            var inner = new BimDataModelStore(jObjFile);
            return inner.Read();
        }

        protected override void DoSave(Database model)
        {
            var jObjFile = new JObjFile();
            var inner = new BimDataModelStore(jObjFile);
            inner.Save(model);
            map.Write(jObjFile.JObject!, folder);
        }
    }

    class FileTools
    {
        public static void WriteToFile(string path, JToken obj)
        {
            WriteToFile(path, obj.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public static void WriteToFile(string path, string text)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, text);
        }
    }
}
