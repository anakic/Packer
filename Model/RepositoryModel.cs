using Packer.Steps;
using System.IO.Compression;

namespace Packer.Model
{
    internal class RepositoryModel
    {
        private readonly string baseFolder;

        List<JsonFileItem> themeFiles;
        List<JsonFileItem> extractedTableFiles;
        List<JsonFileItem> extractedPageFiles;

        private static StepBase? firstStep;
        private static StepBase? lastStep;

        private static void AddStep(StepBase step)
        {
            if (firstStep == null)
                firstStep = step;

            if (lastStep != null)
                lastStep.Next = step;

            lastStep = step;
        }

        public void Load(string pbitFilePath)
        {
            // clear everything except the .git folder
            if (Directory.Exists(baseFolder))
            {
                foreach (var d in Directory.GetDirectories(baseFolder))
                {
                    if (Path.GetFileName(d) == ".git")
                        continue;
                    Directory.Delete(d, true);
                }
                foreach (var f in Directory.GetFiles(baseFolder))
                {
                    File.Delete(f);
                }
            }
            else
                Directory.CreateDirectory(baseFolder);

            ZipFile.ExtractToDirectory(pbitFilePath, baseFolder);

            Initialize();

            firstStep?.Extract(this);
        }

        public void Pack(string pbitFilePath)
        {
            var tempFolderPath = Path.Combine(Path.GetTempPath(), "Packer_temp");

            if (Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);

            IEnumerable<FileItem> itemsToCopy =
                ThemeFiles.Cast<FileItem>()
                .Union(
                    new FileItem[]
                    {
                        DataModelSchemaFile,
                        DiagramLayoutFile,
                        SettingsFile,
                        MetadataFile,
                        VersionFile,
                        LayoutFile,
                        ContentTypesFile
                    });

            itemsToCopy.ToList().ForEach(i => i.CopyRelativeTo(tempFolderPath));
            var model = new RepositoryModel(tempFolderPath);
            firstStep?.Pack(model);

            if (File.Exists(pbitFilePath))
                File.Delete(pbitFilePath);
            ZipFile.CreateFromDirectory(tempFolderPath, pbitFilePath);

            Directory.Delete(tempFolderPath, true);
        }

        public RepositoryModel(string baseFolder)
        {
            this.baseFolder = baseFolder;

            Initialize();
        }

        private void Initialize()
        {
            DataModelSchemaFile = new JsonFileItem(baseFolder, "DataModelSchema");
            DiagramLayoutFile = new JsonFileItem(baseFolder, "DiagramLayout");
            SettingsFile = new JsonFileItem(baseFolder, "Settings");
            MetadataFile = new JsonFileItem(baseFolder, "Metadata");
            VersionFile = new JsonFileItem(baseFolder, "Version");
            LayoutFile = new JsonFileItem(baseFolder, "Layout");
            ContentTypesFile = new XmlFileItem(baseFolder, "[Content_Types].xml");

            string themesFolder = @"Report\StaticResources\SharedResources\BaseThemes";
            themeFiles = Directory
                .GetFiles(Path.Combine(baseFolder, themesFolder))
                .Select(f => new JsonFileItem(baseFolder, GetRelativePath(baseFolder, f)))
                .ToList();

            extractedTableFiles = new List<JsonFileItem>();
            extractedPageFiles = new List<JsonFileItem>();
        }

        private string GetRelativePath(string baseFolder, string f)
        {
            return f.Substring(baseFolder.TrimEnd('\\').Length + 1);
        }

        public JsonFileItem DataModelSchemaFile { get; private set; }
        public JsonFileItem DiagramLayoutFile { get; private set; }
        public JsonFileItem SettingsFile { get; private set; }
        public JsonFileItem MetadataFile { get; private set; }
        public FileItem VersionFile { get; private set; }
        public JsonFileItem LayoutFile { get; private set; }
        public IEnumerable<JsonFileItem> ThemeFiles => themeFiles;

        public List<JsonFileItem> ExtractedTableFiles => extractedTableFiles;
        public List<JsonFileItem> ExtractedPageFiles => extractedPageFiles;

        public XmlFileItem ContentTypesFile { get; private set; }

        public void DeleteSecurityBindingsFile()
        {
            File.Delete(Path.Combine(baseFolder, "SecurityBindings"));
        }
    }
}
