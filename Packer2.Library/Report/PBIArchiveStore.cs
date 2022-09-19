using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Packer2.Library;
using Packer2.Library.Report.Transforms;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace DataModelLoader.Report
{
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
}
