﻿using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Packer2.FileSystem;
using Packer2.Library.DataModel.Customizations;
using Packer2.Library.Report.Stores.Folder.Zones;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel
{
    public class JObjFile : ITextStore
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

    public class FolderDatabaseStore : FolderModelStore<Database>
    {
        class PartitionExpressionsMapping : JsonPropertyZone
        {
            protected override string ElementsSelector => ".partitions[*]";

            protected override string ContainingFolder => "Partitions";

            protected override string PayloadContainingObjectJsonPath => ".source";

            protected override string GetPayloadProperty(JToken obj)
            {
                // a bit hacky, sure, but not worth fixing at the moment
                return GetFileExtension(obj) == "sql"
                    ? "query"
                    : "expression";
            }


            protected override string GetFileName(JToken elem)
                => elem["name"]!.Value<string>()!;

            static Dictionary<string, string> expTypeToExtensionsMap = new Dictionary<string, string>() { { "m", "m" }, { "calculated", "dax" }, { "query", "sql" } };
            protected override string GetFileExtension(JToken elem)
            {
                var type = elem.SelectToken("source.type")!.Value<string>()!;
                return expTypeToExtensionsMap[type];
            }
        }

        class ColumnExpressionsMapping : JsonPropertyZone
        {
            protected override string PayloadContainingObjectJsonPath => "";

            protected override string GetPayloadProperty(JToken obj) => "expression";

            protected override string ContainingFolder => "Columns";

            protected override string ElementsSelector => ".columns[?(@.type=='calculated')]";

            protected override string GetFileName(JToken elem) => $"{(string)elem["name"]!}";

            protected override string GetFileExtension(JToken elem) => "dax";
        }

        class MeasureExpressionsMapping : JsonPropertyZone
        {
            protected override string PayloadContainingObjectJsonPath => "";

            protected override string GetPayloadProperty(JToken obj) => "expression";

            protected override string ContainingFolder => "Measures";

            protected override string ElementsSelector => ".measures[*]";

            protected override string GetFileName(JToken elem) => $"{(string)elem["name"]!}";

            protected override string GetFileExtension(JToken elem) => "dax";
        }

        class ModelExpressionsMapping : JsonPropertyZone
        {
            protected override string PayloadContainingObjectJsonPath => "";

            protected override string GetPayloadProperty(JToken obj) => "expression";

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

        class CalculationItemMapping : JsonPropertyZone
        {
            protected override string PayloadContainingObjectJsonPath => "";

            protected override string GetPayloadProperty(JToken obj) => "expression";

            protected override string ContainingFolder => "Items";

            protected override string ElementsSelector => ".calculationGroup.calculationItems[*]";

            protected override string GetFileName(JToken elem) => (string)elem["name"]!;

            protected override string GetFileExtension(JToken elem) => "dax";
        }

        class CalculationGroupMapping : JsonElementZone
        {
            protected override string ElementsSelector => ".model.tables[*]";

            protected override string ContainingFolder => "CalculationGroups";

            protected override string GetSubfolderForElement(JToken elem)
                => (string)elem["name"]!;

            protected override string GetFileName(JToken elem) => "group";

            protected override string GetFileExtension(JToken elem) => "json";

            protected override IEnumerable<MappingZone> ChildMappings { get; }

            protected override bool FilterSelectedElements(JToken element)
            {
                return element["calculationGroup"] != null;
            }

            public CalculationGroupMapping()
            {
                ChildMappings = new MappingZone[]
                {
                    new CalculationItemMapping()
                };
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

            protected override bool FilterSelectedElements(JToken element)
            {
                return element["calculationGroup"] == null;
            }

            public TablesMapping()
            {
                ChildMappings = new MappingZone[]
                {
                    new PartitionExpressionsMapping(),
                    new ColumnExpressionsMapping(),
                    new MeasureExpressionsMapping(),
                };
            }
        }

        class BimMappedRepository : JsonMappedFolder
        {
            public BimMappedRepository()
            {
                Mappings = new MappingZone[]
                {
                    new TablesMapping(),
                    new ModelExpressionsMapping(),
                    new CalculationGroupMapping(),
                };
            }

            protected override IEnumerable<MappingZone> Mappings { get; }

            protected override string RootFileName => "database.json";

            public bool RepositoryExists(IFileSystem fs)
                => fs.FileExists(RootFileName);
        }

        BimMappedRepository map = new BimMappedRepository();

        // todo: use logger
        private readonly ILogger<FolderDatabaseStore> logger;

        public string Customization { get; set; }

        public FolderDatabaseStore(string folderPath, string? customization = null, ILogger<FolderDatabaseStore>? logger = null)
            : this(new LocalFileSystem(folderPath), customization, logger)
        { }

        IFileSystem originalFileSystem;
        public FolderDatabaseStore(IFileSystem fileSystem, string? customization = null, ILogger<FolderDatabaseStore>? logger = null)
            : base(customization == null ? fileSystem : new CustFileSystem(fileSystem, customization))
        {
            originalFileSystem = fileSystem;
            this.logger = logger ?? new DummyLogger<FolderDatabaseStore>();
        }

        protected override Database DoRead(IFileSystem fileSystem)
        {
            Database db = ReadDatabase(fileSystem);

            var ignoreFile = ReadIgnoreFile();
            var rules = new IgnoreFileParser().Parse(ignoreFile);
            var filter = new DataModelFilter(rules);
            filter.Crop(db);

            return db;
        }

        protected override void DoSave(Database model, IFileSystem fileSystem)
        {
            if (map.RepositoryExists(fileSystem))
            {
                Database originalFullDb = ReadDatabase(fileSystem);

                var ignoreFile = ReadIgnoreFile();
                var rules = new IgnoreFileParser().Parse(ignoreFile);
                var filter = new DataModelFilter(rules);
                filter.Extend(model, originalFullDb);
            }

            var jObjFile = new JObjFile();
            var inner = new BimDataModelStore(jObjFile);
            inner.Save(model);
            map.Write(jObjFile.JObject!, fileSystem);
        }

        private Database ReadDatabase(IFileSystem fileSystem)
        {
            var jobject = map.Read(fileSystem);
            var jObjFile = new JObjFile(jobject);
            var inner = new BimDataModelStore(jObjFile);
            var db = inner.Read();
            return db;
        }
    }
}
