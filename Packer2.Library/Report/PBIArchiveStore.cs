using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Packer2.Library;
using Packer2.Library.Tools;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using SaveOptions = System.Xml.Linq.SaveOptions;

namespace DataModelLoader.Report
{
    interface IKnownArchiveFileLoader
    {
        string RelativePath { get; }
        void LoadIntoModel(byte[] bytes, PowerBIReport model);
        byte[]? ReadFromModel(PowerBIReport model);
    }

    class SecurityBindingsFileLoader : IKnownArchiveFileLoader
    {
        public string RelativePath => "SecurityBindings";
        public void LoadIntoModel(byte[] bytes, PowerBIReport model) { }
        public byte[]? ReadFromModel(PowerBIReport model) => null;
    }

    abstract class TextFileLoaderBase<T> : IKnownArchiveFileLoader
    {
        private readonly Func<PowerBIReport, T> getValueFunc;
        private readonly Action<PowerBIReport, T> setValueAction;
        private readonly Encoding encoding;
        private readonly bool hasPreamble;
        private readonly ILogger logger;

        public string RelativePath { get; }

        protected TextFileLoaderBase(string relativePath, Func<PowerBIReport, T> getValueFunc, Action<PowerBIReport, T> setValueAction, Encoding encoding, bool hasPreamble, ILogger logger)
        {
            this.encoding = encoding;
            this.hasPreamble = hasPreamble;
            this.logger = logger;
            RelativePath = relativePath;
            this.getValueFunc = getValueFunc;
            this.setValueAction = setValueAction;
        }

        public void LoadIntoModel(byte[] bytes, PowerBIReport model)
        {
            logger.LogTrace("Loading file {path} into model", RelativePath);

            if (hasPreamble)
            {
                var preamble = encoding.GetPreamble();
                for (int i = 0; i < preamble.Length; i++)
                {
                    if (preamble[i] != bytes[i])
                        throw new Exception($"Preable for encoding {encoding.EncodingName} expected but not found or incorrect");
                }
                // note: is span<T> better used for stuff like this?
                bytes = bytes.Skip(preamble.Length).ToArray();
            }

            var str = encoding.GetString(bytes);
            T value = ConvertFromString(str);
            setValueAction(model, value);
        }

        public byte[]? ReadFromModel(PowerBIReport model)
        {
            var value = getValueFunc(model);
            if (value != null)
            {
                var str = ConvertToString(value);
                var bytes = encoding.GetBytes(str);
                return hasPreamble 
                    ? encoding.GetPreamble().Concat(bytes).ToArray() 
                    : encoding.GetBytes(str);
            }
            else
                return null;
        }

        protected abstract string ConvertToString(T value);
        protected abstract T ConvertFromString(string str);
    }

    class TextFileLoader : TextFileLoaderBase<string>
    {
        public TextFileLoader(string relativePath, Func<PowerBIReport, string> getValueFunc, Action<PowerBIReport, string> setValueAction, Encoding encoding, bool hasPreamble, ILogger logger) 
            : base(relativePath, getValueFunc, setValueAction, encoding, hasPreamble, logger)
        {
        }

        protected override string ConvertFromString(string str) => str;
        protected override string ConvertToString(string value) => value;
    }

    class XmlFileLoader : TextFileLoaderBase<XDocument>
    {
        private readonly bool indentation;

        public XmlFileLoader(string expectedRelativeFilePath, Action<PowerBIReport, XDocument> setValueAction, Func<PowerBIReport, XDocument> getValueFunc, Encoding encoding, bool hasPreamble, bool indentation, ILogger logger)
            : base(expectedRelativeFilePath, getValueFunc, setValueAction, encoding, hasPreamble, logger)
        {
            this.indentation = indentation;
        }
        protected override XDocument ConvertFromString(string str) => XDocument.Parse(str);
        protected override string ConvertToString(XDocument data) => data.Declaration?.ToString() + data.ToString(indentation ? SaveOptions.None : SaveOptions.DisableFormatting);
    }

    class JsonFileLoader : TextFileLoaderBase<JObject>
    {
        private readonly bool indentation;

        public JsonFileLoader(string expectedRelativeFilePath, Action<PowerBIReport, JObject> setValueAction, Func<PowerBIReport, JObject> getValueFunc, Encoding encoding, bool hasPreamble, bool indentation, ILogger logger)
            : base(expectedRelativeFilePath, getValueFunc, setValueAction, encoding, hasPreamble, logger)
        {
            this.indentation = indentation;
        }

        protected override JObject ConvertFromString(string text) => JObject.Parse(text);
        protected override string ConvertToString(JObject data) => data.ToString(indentation ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
    }

    class ContentTypesFileLoader : XmlFileLoader
    {
        public ContentTypesFileLoader(ILogger logger)
            : base("[Content_Types].xml", (m, v) => m.Content_Types = v, m => m.Content_Types, Encoding.UTF8, true, false, logger)
        {
        }

        protected override XDocument ConvertFromString(string text)
        {
            var document = base.ConvertFromString(text);
            document.Descendants(XName.Get("Override", @"http://schemas.openxmlformats.org/package/2006/content-types"))
                .SingleOrDefault(xe => xe.Attribute("PartName")?.Value == "/SecurityBindings")
                ?.Remove();
            return document;
        }

        protected override string ConvertToString(XDocument data) => base.ConvertToString(data);
    }

    public class PBIArchiveStore : IModelStore<PowerBIReport>
    {
        IKnownArchiveFileLoader[] knowFileLoaders;

        private readonly string archivePath;
        private readonly ILogger<PBIArchiveStore> logger;

        public PBIArchiveStore(string archivePath, ILogger<PBIArchiveStore>? logger = null)
        {
            this.archivePath = archivePath;
            this.logger = logger ?? new DummyLogger<PBIArchiveStore>();
            knowFileLoaders = new IKnownArchiveFileLoader[]
            {
                new JsonFileLoader("DiagramLayout", (m, v) => m.DiagramLayout = v, m => m.DiagramLayout, Encoding.Unicode, false, false, logger),
                new JsonFileLoader("DataModelSchema", (m, v) => m.DataModelSchemaFile = v, m => m.DataModelSchemaFile, Encoding.Unicode, false, true, logger),
                new JsonFileLoader("Metadata", (m, v) => m.Metadata = v, m => m.Metadata, Encoding.Unicode, false, false, logger),
                new JsonFileLoader("Settings", (m, v) => m.Settings = v, m => m.Settings, Encoding.Unicode, false, false, logger),
                new TextFileLoader("Version", m => m.Version, (m, v) => m.Version= v, Encoding.Unicode, false, logger),
                new JsonFileLoader("Connections", (m, v) => m.Connections = v, m => m.Connections, Encoding.UTF8, false, false, logger),
                new JsonFileLoader("Report\\Layout", (m, v) => m.Layout = v, m => m.Layout, Encoding.Unicode, false, false, logger),
                new XmlFileLoader("Report\\LinguisticSchema", (m, v) => m.Report_LinguisticSchema= v, m => m.Report_LinguisticSchema, Encoding.Unicode, false, false, logger),
                new ContentTypesFileLoader(logger),
                new SecurityBindingsFileLoader(),
            };
        }
        public PowerBIReport Read()
        {
            using (var tempFolder = new TempFolder("Pbi_Arthive_Unzip_Temp"))
            {
                var pathComparer = new RelativePathEqualityComparer(tempFolder.Path);
                ZipFile.ExtractToDirectory(archivePath, tempFolder.Path);
                var files = Directory.GetFiles(tempFolder.Path, "*", SearchOption.AllDirectories);
                var model = new PowerBIReport();
                files.ToList().ForEach(f =>
                {
                    var bytes = File.ReadAllBytes(f);
                    var relativePath = PathTools.GetRelativePath(f, tempFolder.Path);
                    var loader = knowFileLoaders.FirstOrDefault(l => pathComparer.Equals(f, l.RelativePath));
                    if (loader != null)
                        loader.LoadIntoModel(bytes, model);
                    else
                        model.Blobs[relativePath] = bytes;
                });

                return model;
            }
        }

        public void Save(PowerBIReport model)
        {
            using (var tempFolder = new TempFolder("Pbi_Arthive_Zip_Temp"))
            {
                foreach (var l in knowFileLoaders)
                {
                    var bytes = l.ReadFromModel(model);
                    if (bytes != null)
                    {
                        var path = Path.Combine(tempFolder.Path, l.RelativePath);
                        WriteToFile(path, bytes);
                    }
                }

                foreach (var b in model.Blobs)
                    WriteToFile(Path.Combine(tempFolder.Path, b.Key), b.Value);

                // create zip from output folder
                if (File.Exists(archivePath))
                    File.Delete(archivePath);
                ZipFile.CreateFromDirectory(tempFolder.Path, archivePath);
            }
        }

        private void WriteToFile(string path, byte[] bytes)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllBytes(path, bytes);
        }
    }
}
