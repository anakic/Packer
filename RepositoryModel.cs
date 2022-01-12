using Newtonsoft.Json.Linq;
using System.Text;
using System.Xml.Linq;

namespace Packer
{
    abstract class FileSystemItem
    {
        public string Path { get; }

        public FileSystemItem(string path)
        {
            Path = path;
        }

        //public abstract void Delete();
    }

    //class FolderItem<T> : FileSystemItem where T : FileSystemItem
    //{
    //    Lazy<List<T>> items = new Lazy<List<T>>();

    //    string absolutePath;
    //    public FolderItem(string basePath, string relativePath)
    //        : base(relativePath)
    //    {
    //        absolutePath = System.IO.Path.Combine(basePath, relativePath);
    //    }

    //    public IEnumerable<T> Items => xxx;
    //    public void AddItem(T item) 
    //    {
    //    }
    //    public void RemoveItem(T item) { }

    //    //public override void Delete()
    //    //{
    //    //    Directory.Delete(absolutePath, true);
    //    //}
    //}

    class FileItem : FileSystemItem
    {
        protected string AbsolutePath { get; }

        public FileItem(string basePath, string relativePath)
            : base(relativePath)
        {
            AbsolutePath = System.IO.Path.Combine(basePath, relativePath);
        }

        //public override void Delete()
        //{
        //    File.Delete(AbsolutePath);
        //}
        
        public string ReadAsString(Encoding? encoding = null) 
        {
            if (encoding != null)
                return File.ReadAllText(AbsolutePath, encoding);
            else
                return File.ReadAllText(AbsolutePath);
        }

        public byte[] ReadBytes() 
        {
            return File.ReadAllBytes(AbsolutePath);
        }

        public void Save(string str, Encoding? encoding = null)
        {
            if (encoding != null)
                File.WriteAllText(AbsolutePath, str, encoding);
            else
                File.WriteAllText(AbsolutePath, str);
        }

        public void Save(byte [] bytes)
        {
            File.WriteAllBytes(AbsolutePath, bytes);
        }

        public void CopyRelativeTo(string otherBasePath)
        {
            File.Copy(AbsolutePath, System.IO.Path.Combine(otherBasePath, Path));
        }
    }

    class JsonFileItem : FileItem
    {
        Lazy<JObject> lazyObj;
        public JsonFileItem(string basePath, string relativePath) : base(basePath, relativePath)
        {
            lazyObj = new Lazy<JObject>(() => ParseJsonStr(ReadBytes()));
        }

        public void Modify(Action<JObject> action, Newtonsoft.Json.Formatting formatting) 
        {
            action(lazyObj.Value);
            var str = lazyObj.Value.ToString(formatting);
            var bytes = Encoding.Unicode.GetBytes(str);
            Save(bytes);
        }

        static JObject ParseJsonStr(byte[] bytes)
        {
            var encodingsToTry = new[] { Encoding.Unicode, Encoding.UTF8 };
            foreach (var enc in encodingsToTry)
            {
                try
                {
                    return JObject.Parse(enc.GetString(bytes));
                }
                catch
                {
                    // ignored
                }
            }
            throw new Exception("Unknown encoding or error in json string");
        }
    }

    class XmlFileItem : FileItem
    {
        Lazy<XDocument> lazyDocument;

        public XmlFileItem(string basePath, string relativePath) : base(basePath, relativePath)
        {
            lazyDocument = new Lazy<XDocument>(() => XDocument.Parse(ReadAsString()));
        }

        public void Modify(Action<XDocument> documentAction)
        {
            documentAction(lazyDocument.Value);
            lazyDocument.Value.Save(AbsolutePath);
        }
    }

    internal class RepositoryModel
    {
        private readonly string baseFolder;

        List<JsonFileItem> themeFiles;
        List<JsonFileItem> extractedTableFiles;
        List<JsonFileItem> extractedPageFiles;

        public RepositoryModel(string baseFolder)
        {
            this.baseFolder = baseFolder;

            DataModelSchemaFile = new JsonFileItem(baseFolder, "DataModelSchema");
            DiagramLayoutFile = new JsonFileItem(baseFolder, "DiagramLayout");
            SettingsFile = new JsonFileItem(baseFolder, "Settings");
            MetadataFile = new JsonFileItem(baseFolder, "Metadata");
            VersionFile = new JsonFileItem(baseFolder, "Version");
            LayoutFile = new JsonFileItem(baseFolder, "Layout");

            string themesFolder = @"Report\StaticResources\SharedResources\BaseThemes";
            themeFiles = Directory
                .GetFiles(Path.Combine(baseFolder, themesFolder))
                .Select(f => new JsonFileItem(baseFolder, GetRelativePath(baseFolder, f)))
                .ToList();
        }

        private string GetRelativePath(string baseFolder, string f)
        {
            return f.Substring(baseFolder.TrimEnd('\\').Length + 1);
        }

        public JsonFileItem DataModelSchemaFile { get; }
        public JsonFileItem DiagramLayoutFile { get; }
        public JsonFileItem SettingsFile { get; }
        public JsonFileItem MetadataFile { get; }
        public FileItem VersionFile { get; }
        public JsonFileItem LayoutFile { get; }
        public IEnumerable<JsonFileItem> ThemeFiles => themeFiles;

        public List<JsonFileItem> ExtractedTableFiles => extractedTableFiles;
        public List<JsonFileItem> ExtractedPageFiles => extractedPageFiles;

        public XmlFileItem ContentTypesFile { get; }

        public void DeleteSecurityBindingsFile()
        {
            File.Delete(Path.Combine(baseFolder, "SecurityBindings"));
        }
    }
}
