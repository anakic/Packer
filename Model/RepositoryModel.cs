using Newtonsoft.Json.Linq;
using Packer.Storage;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Packer.Model
{
    /// <summary>
    /// Represents the repository folder. Exposes the files contained in the repository via strongly typed properties.
    /// Knows about the structure of the folder but know nothing of the contents and meanings of those files.
    /// </summary>
    internal class RepositoryModel
    {
        string tablesFolder = @"Tables";
        string pagesFolder = @"Report\Pages";
        string themesFolder = @"Report\StaticResources\SharedResources\BaseThemes";
        string resourcesFolder = @"Report\StaticResources\RegisteredResources";

        List<JsonFileItem> themeFiles;
        List<BinaryFileItem> resourceFiles;
        List<JsonFileItem> extractedTableFiles;
        List<JsonFileItem> extractedPageFiles;

        public RepositoryModel(IFilesStore source)
        {
            DataModelSchemaFile = ReadJson(source, "DataModelSchema");
            DiagramLayoutFile = ReadJson(source, "DiagramLayout");
            SettingsFile = ReadJson(source, "Settings");
            MetadataFile = ReadJson(source, "Metadata");
            SecurityBindings = ReadBinary(source, "SecurityBindings");
            VersionFile = ReadText(source, "Version");
            ContentTypesFile = ReadXml(source, "[Content_Types].xml");
            LayoutFile = ReadJson(source, @"Report\Layout");

            TableSchemaFile = ReadJsonSchema(@"Tables\table-schema.json");

            themeFiles = new List<JsonFileItem>();
            foreach (var themeFile in source.GetFiles(themesFolder))
                themeFiles.Add(ReadJson(source, themeFile)!);

            extractedTableFiles = new List<JsonFileItem>();
            foreach (var tableFile in source.GetFiles(tablesFolder).Where(f => !f.ToLower().EndsWith("-schema.json")))
                extractedTableFiles.Add(ReadJson(source, tableFile)!);

            extractedPageFiles = new List<JsonFileItem>();
            foreach (var pageFile in source.GetFiles(pagesFolder))
                extractedPageFiles.Add(ReadJson(source, pageFile)!);

            resourceFiles = new List<BinaryFileItem>();
            foreach (var resFile in source.GetFiles(resourcesFolder))
                resourceFiles.Add(ReadBinary(source, resFile)!);
        }

        // todo: return JsonSchemaFile (exposes JSchema instead of JObject)
        private JsonFileItem ReadJsonSchema(string schemaFileDestinationPath)
        {
            var schemaFileName = Path.GetFileName(schemaFileDestinationPath);
            var schemasSourceFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Schemas");
            var absPath = Path.Combine(schemasSourceFolder, schemaFileName);
            var jObj = JObject.Parse(File.ReadAllText(absPath));
            return new JsonFileItem(schemaFileDestinationPath, jObj);
        }

        internal JsonFileItem GetExtractedJsonFile(string v)
        {
            return extractedPageFiles.Union(extractedTableFiles).First(f => string.Equals(f.Path, v));
        }

        public void WriteTo(IFilesStore fileSystem, bool forHuman)
        {
            IEnumerable<FileSystemItem> filesToSave = themeFiles.Cast<FileSystemItem>()
                .Union(resourceFiles)
                .Union(new FileSystemItem?[]
                {
                    DataModelSchemaFile,
                    DiagramLayoutFile,
                    SettingsFile,
                    ContentTypesFile,
                    SecurityBindings,
                    MetadataFile,
                    LayoutFile,
                    VersionFile
                })
                .Where(jf => jf != null)
                .Select(jf => jf!)
                .ToList();

            if (forHuman)
            {
                filesToSave = filesToSave
                    .Union(extractedTableFiles)
                    .Union(extractedPageFiles)
                    .Union(new[]
                    {
                        TableSchemaFile
                    });
            }

            foreach (var file in filesToSave)
            {
                if (forHuman)
                    file.SaveForHuman(fileSystem);
                else
                    file.SaveForMachine(fileSystem);
            }
        }

        private BinaryFileItem? ReadBinary(IFilesStore source, string path)
        {
            if (source.FileExists(path))
                return new BinaryFileItem(path, source.ReadAsBytes(path));
            return null;
        }

        private TextFileItem? ReadText(IFilesStore source, string path)
        {
            if (source.FileExists(path))
                return new TextFileItem(path, source.ReadAsText(path, Encoding.Unicode));
            return null;
        }

        private JsonFileItem? ReadJson(IFilesStore fileSystem, string path)
        {
            if (fileSystem.FileExists(path))
                return JsonFileItem.Read(path, fileSystem);
            return null;
        }

        private XmlFileItem? ReadXml(IFilesStore fileSystem, string path)
        {
            if (fileSystem.FileExists(path))
                return new XmlFileItem(path, XDocument.Parse(fileSystem.ReadAsText(path)));
            return null;
        }

        public JsonFileItem? DataModelSchemaFile { get; set; }
        public JsonFileItem? DiagramLayoutFile { get; set; }
        public XmlFileItem? ContentTypesFile { get; set; }
        public JsonFileItem? SettingsFile { get; set; }
        public JsonFileItem? MetadataFile { get; set; }
        public BinaryFileItem? SecurityBindings { get; set; }
        public TextFileItem? VersionFile { get; set; }
        public JsonFileItem? LayoutFile { get; set; }

        public JsonFileItem TableSchemaFile { get; set; }


        public IEnumerable<JsonFileItem> ThemeFiles => themeFiles;

        public IEnumerable<JsonFileItem> ExtractedTableFiles => extractedTableFiles;
        public IEnumerable<JsonFileItem> ExtractedPageFiles => extractedPageFiles;

        public JsonFileItem AddExtractedTableFile(string tableName, JObject jObject)
        {
            var file = new JsonFileItem(Path.Combine(tablesFolder, tableName), jObject);
            extractedTableFiles.Add(file);
            return file;
        }

        public void ClearExtractedTables()
            => extractedTableFiles.Clear();

        public void ClearExtractedPages()
            => extractedPageFiles.Clear();

        public JsonFileItem AddExtractedPageFile(string pageName, JObject jObject)
        {
            var file = new JsonFileItem(Path.Combine(pagesFolder, pageName), jObject);
            extractedTableFiles.Add(file);
            return file;
        }

        // Gets all json files that are part of the model (includes extracted files)
        public IEnumerable<JsonFileItem> GetAllJsonFiles()
        {
            return ThemeFiles
                .Union(extractedTableFiles)
                .Union(extractedPageFiles)
                .Union(new[]
                {
                    SettingsFile!,
                    LayoutFile!,
                    DataModelSchemaFile!,
                    DiagramLayoutFile!,
                    MetadataFile!
                });
        }
    }
}
