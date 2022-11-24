﻿using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Packer2.FileSystem;
using Packer2.Library.Report.Stores.Folder.Transforms;
using Packer2.Library.Tools;
using System.Xml.Linq;

namespace Packer2.Library.Report.Stores.Folder
{
    public partial class ReportFolderStore : FolderModelStore<PowerBIReport>
    {
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
        

        public bool EnableQueryMinification { get; set; } = true;

        public bool EnableBookmarkSimplification { get; set; } = true;

        public bool EnableStripVisualState { get; set; } = true;

        public ReportFolderStore(string folderPath, ILogger<ReportFolderStore>? logger = null)
            : this(new LocalFileSystem(folderPath), logger)
        {
        }

        public ReportFolderStore(IFileSystem fileSystem, ILogger<ReportFolderStore>? logger = null)
            : base(fileSystem)
        {
            this.logger = logger ?? new DummyLogger<ReportFolderStore>();

            this.fileSystem = fileSystem;
        }

        private IEnumerable<IJObjTransform> GetTransforms()
        {
            // note: ordering is important because unstuff changes selectors
            // that said, I could certainly optimize things by running minification after unstuff
            // becaise this way I unstuff twice.

            if (EnableQueryMinification)
                yield return new MinifyQueriesTransform(fileSystem, logger);

            yield return new UnstuffTransform();
            yield return new ConsolidateOrderingTransform();

            if(EnableStripVisualState)
                yield return new StripVisualStatePropertiesTransform();

            if (EnableBookmarkSimplification)
                yield return new SimplifyBookmarks();
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
            foreach(var t in GetTransforms().Reverse())
            {
                logger.LogInformation("Restoring report transformation '{transformation}'", t.GetType().Name);
                t.Restore(rpt);
            }
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
            foreach(var t  in GetTransforms())
            {
                logger.LogInformation("Applying report transformation '{transformation}'", t.GetType().Name);
                t.Transform(layoutJObjClone);
            }
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