using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel
{
    public class FolderDatabaseStore : FolderModelStore<Database>
    {
        class PartitionExpressionsMapping : JsonPropertyZone
        {
            protected override string ElementsSelector => ".partitions[*]";

            protected override string ContainingFolder => "Partitions";

            protected override string PayloadContainingObjectJsonPath => ".source";

            protected override string PayloadProperty => "expression";


            protected override string GetFileName(JToken elem)
                => elem["name"]!.Value<string>()!;
            
            static Dictionary<string, string> expTypeToExtensionsMap = new Dictionary<string, string>() { { "m", "m" }, { "calculated", "dax" } };
            protected override string GetFileExtension(JToken elem)
            {
                var type = elem.SelectToken("source.type")!.Value<string>()!;
                return expTypeToExtensionsMap[type];
            }
        }

        class ColumnExpressionsMapping : JsonPropertyZone
        {
            protected override string PayloadContainingObjectJsonPath => "";

            protected override string PayloadProperty => "expression";

            protected override string ContainingFolder => "Columns";

            protected override string ElementsSelector => ".columns[?(@.type=='calculated')]";

            protected override string GetFileName(JToken elem) => $"{(string)elem["name"]!}";

            protected override string GetFileExtension(JToken elem) => "dax";
        }

        class MeasureExpressionsMapping : JsonPropertyZone
        {
            protected override string PayloadContainingObjectJsonPath => "";

            protected override string PayloadProperty => "expression";

            protected override string ContainingFolder => "Measures";

            protected override string ElementsSelector => ".measures[*]";

            protected override string GetFileName(JToken elem) => $"{(string)elem["name"]!}";
            
            protected override string GetFileExtension(JToken elem) => "dax";
        }

        class ModelExpressionsMapping : JsonPropertyZone
        {
            protected override string PayloadContainingObjectJsonPath => "";

            protected override string PayloadProperty => "expression";

            protected override string ContainingFolder => "MQueries";

            protected override string ElementsSelector => ".model.expressions[*]";

            protected override string GetFileName(JToken elem) => (string)elem["name"]!;

            static Dictionary<string, string> expTypeToExtensionsMap = new Dictionary<string, string>() { { "m", "m" }, { "calculated", "dax" } };
            protected override string GetFileExtension(JToken elem)
            {
                var type = elem.SelectToken("kind")!.Value<string>()!;
                return expTypeToExtensionsMap[type];
            }
        }

        class TablesMapping : JsonElementZone
        {
            protected override string ElementsSelector => ".model.tables[*]";

            protected override string ContainingFolder => "Tables";

            protected override string GetSubfolderForElement(JToken elem)
                => (string)elem["name"]!;

            protected override string GetFileName(JToken elem) => "table";

            protected override string GetFileExtension(JToken elem) => "json";

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
}
