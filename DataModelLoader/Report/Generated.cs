using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DataModelLoader.Report
{
    public class PBIFolderLoader : IModelStore<PowerBIReport>
    {
        private readonly string folderPath;

        public PBIFolderLoader(string folderPath)
        {
            this.folderPath = folderPath;
        }

        public PowerBIReport Read()
        {
            throw new NotImplementedException();
        }

        private void ClearFolder()
        {
            if (Directory.Exists(folderPath))
            {
                foreach (var childDir in Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly))
                {
                    // do not remove the .git folder
                    if (Path.GetFileName(childDir) != ".git")
                        Directory.Delete(childDir, true);
                }

                foreach (var file in Directory.GetFiles(folderPath))
                    File.Delete(file);
            }
        }


        public void Save(PowerBIReport model)
        {
            ClearFolder();

            foreach (var kvp in model.Blobs)
            {
                var path = Path.Combine(folderPath, kvp.Key);
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllBytes(path, kvp.Value);
            }

            // todo: define or reuse constants for file names
            File.WriteAllText(Path.Combine(folderPath, "Connections.json"), model.Connections.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "[Content_Types].xml"), model.Content_Types.ToString());
            File.WriteAllText(Path.Combine(folderPath, "DataModelSchema.json"), model.DataModelSchemaFile.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "DiagramLayout.json"), model.DiagramLayout.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "Metadata.json"), model.Metadata.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "Settings.json"), model.Settings.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "Report\\Layout.json"), JsonConvert.SerializeObject(model.Layout, Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "Report\\LinguisticSchema.xml"), model.Report_LinguisticSchema.ToString());
        }
    }

    public class PBIArchiveStore : IModelStore<PowerBIReport>, IDisposable
    {
        private readonly string tempFolderPath;
        private readonly string archivePath;

        public PBIArchiveStore(string archivePath)
        {
            this.archivePath = archivePath;
            tempFolderPath = Path.Combine(Path.GetTempPath(), "PbiArthiveUnzipTemp");
        }

        public void Dispose()
        {
            ClearFolder();
        }

        private void ClearFolder()
        {
            if (Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);
        }

        class RelativePathEqualityComparer : IEqualityComparer<string>
        {
            private readonly string baseFolder;
            Guid g = new Guid("{CF3A12B3-1241-4639-8781-DAB2A36CE426}");

            public RelativePathEqualityComparer(string baseFolder)
            {
                this.baseFolder = baseFolder;
            }

            public bool Equals(string x, string y)
            {
                return Path.GetFullPath(EnsureAbsolute(x)) == Path.GetFullPath(EnsureAbsolute(y));
            }

            public int GetHashCode(string obj)
            {
                return (Path.GetFullPath(EnsureAbsolute(obj)) + g).GetHashCode();
            }

            private string EnsureAbsolute(string path)
                => Path.IsPathRooted(path) ? path : Path.Combine(baseFolder, path);
        }

        string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        const string layoutFile = "Report\\Layout";
        const string dataModelSchemaFileName = "DataModelSchema";
        const string diagramLayoutFileName = "DiagramLayout";
        const string metadata = "Metadata";
        const string settings = "Settings";
        const string version = "Version";
        const string connections = "Connections";
        const string reportLinguisticSchema = "Report\\LinguisticSchema";
        const string contentTypesFilePath = "[Content_Types].xml";
        const string securityBindingsPath = "SecurityBindings";

        public PowerBIReport Read()
        {
            RelativePathEqualityComparer pathComparer = new RelativePathEqualityComparer(tempFolderPath);

            ClearFolder();

            ZipFile.ExtractToDirectory(archivePath, tempFolderPath);

            var files = Directory.GetFiles(tempFolderPath, "*", SearchOption.AllDirectories);

            var unicodeEncodedFiles = new[] { dataModelSchemaFileName, diagramLayoutFileName, metadata, settings, version, layoutFile, reportLinguisticSchema }.ToHashSet(pathComparer);

            var model = new PowerBIReport();

            files.ToList().ForEach(f =>
            {
                var relativePath = GetRelativePath(f, tempFolderPath);

                if (pathComparer.Equals(contentTypesFilePath, f))
                {
                    var doc = XDocument.Parse(File.ReadAllText(f));
                    doc.Descendants(XName.Get("Override", @"http://schemas.openxmlformats.org/package/2006/content-types"))
                        .SingleOrDefault(xe => xe.Attribute("PartName")?.Value == "/SecurityBindings")
                        ?.Remove();
                    model.Content_Types = doc;
                }
                else if (pathComparer.Equals(securityBindingsPath, f))
                {
                    // skip this file
                }
                else if (pathComparer.Equals(connections, f))
                {
                    string fileContents = File.ReadAllText(f);
                    model.Connections = JObject.Parse(fileContents);
                }
                else if (unicodeEncodedFiles.Contains(f))
                {
                    string fileContents = File.ReadAllText(f, Encoding.Unicode);

                    if (pathComparer.Equals(f, layoutFile))
                        model.Layout = JsonConvert.DeserializeObject<ReportLayout>(fileContents);
                    else if (pathComparer.Equals(f, dataModelSchemaFileName))
                        model.DataModelSchemaFile = JObject.Parse(fileContents);
                    else if (pathComparer.Equals(f, diagramLayoutFileName))
                        model.DiagramLayout = JObject.Parse(fileContents);
                    else if (pathComparer.Equals(f, metadata))
                        model.Metadata = JObject.Parse(fileContents);
                    else if (pathComparer.Equals(f, settings))
                        model.Settings = JObject.Parse(fileContents);
                    else if (pathComparer.Equals(f, connections))
                        model.Connections = JObject.Parse(fileContents);
                    else if (pathComparer.Equals(f, reportLinguisticSchema))
                        model.Report_LinguisticSchema = XDocument.Parse(fileContents);
                    else
                        model.Blobs[relativePath] = Encoding.UTF8.GetBytes(fileContents);
                }
                else
                {
                    model.Blobs[relativePath] = File.ReadAllBytes(f);
                }
            });

            return model;
        }

        public void Save(PowerBIReport model)
        {
            throw new NotImplementedException();
        }
    }


    public class PowerBIReport
    {
        public JObject DataModelSchemaFile { get; set; }
        public JObject DiagramLayout { get; set; }
        public JObject Metadata { get; set; }
        public JObject Settings { get; set; }
        public JObject Connections { get; set; }

        public XDocument Report_LinguisticSchema { get; set; }
        public XDocument Content_Types { get; set; }

        public Dictionary<string, byte[]> Blobs { get; } = new Dictionary<string, byte[]>();

        public ReportLayout Layout { get; set; }
    }

    public class ResourceItem
    {
        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Pod
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("boundSection")]
        public string BoundSection { get; set; }

        [JsonProperty("parameters")]
        public string Parameters { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("config")]
        public string Config { get; set; }
    }

    public class ResourcePackageContainer
    {
        [JsonProperty("resourcePackage")]
        public ResourcePackage ResourcePackage { get; set; }
    }

    public class ResourcePackage
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("items")]
        public List<ResourceItem> Items { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
    }

    public class ReportLayout
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filters")]
        public string Filters { get; set; }

        [JsonProperty("resourcePackages")]
        public List<ResourcePackageContainer> ResourcePackages { get; set; }

        [JsonProperty("sections")]
        public List<Section> Sections { get; set; }

        [JsonProperty("config")]
        public string Config { get; set; }

        [JsonProperty("layoutOptimization")]
        public int LayoutOptimization { get; set; }

        [JsonProperty("publicCustomVisuals")]
        public List<string> PublicCustomVisuals { get; set; }

        [JsonProperty("pods")]
        public List<Pod> Pods { get; set; }
    }

    public class Section
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("filters")]
        public string Filters { get; set; }

        [JsonProperty("ordinal")]
        public int Ordinal { get; set; }

        [JsonProperty("visualContainers")]
        public List<VisualContainer> VisualContainers { get; set; }

        [JsonProperty("config")]
        public string Config { get; set; }

        [JsonProperty("displayOption")]
        public int DisplayOption { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }
    }

    public class VisualContainer
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public int Z { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("config")]
        public string Config { get; set; }

        [JsonProperty("tabOrder")]
        public int TabOrder { get; set; }

        [JsonProperty("filters")]
        public string Filters { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("dataTransforms")]
        public string DataTransforms { get; set; }
    }
}
