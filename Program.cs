using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Packer
{
    class Program
    {
        static Encoding unicode = Encoding.Unicode;

        static void Main(string[] args)
        {
            var operation = args[0];
            if (operation == "pack")
                Pack(args[1], args[2]);

            else if (operation == "unpack")
                Unpack(args[1], args[2]);
        }
        static void Pack(string sourceFolder, string outputFilePath)
        {
            var tempFolderPath = Path.Combine(Path.GetTempPath(), "Packer_temp");

            if (Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);

            foreach (var f in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
            {
                if (f.Contains(".git\\"))
                    continue;

                var relativePath = f.Substring(sourceFolder.TrimEnd('\\').Length + 1);
                var destinationPath = Path.Combine(tempFolderPath, relativePath);

                var destinationDir = Path.GetDirectoryName(destinationPath) ?? throw new Exception("Invalid path");
                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                File.Copy(f, destinationPath);
            }

            Directory.GetFiles(tempFolderPath, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(tempFolderPath, "Report"), "*", SearchOption.AllDirectories))
                .ToList()
                .ForEach(f =>
                {
                    var str = File.ReadAllText(f);
                    var obj = JObject.Parse(str);
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.None);
                    var bytes = unicode.GetBytes(formatted);
                    File.WriteAllBytes(f, bytes);
                });

            if(File.Exists(outputFilePath))
                File.Delete(outputFilePath);
            ZipFile.CreateFromDirectory(tempFolderPath, outputFilePath);

            Directory.Delete(tempFolderPath, true);
        }

        static void Unpack(string file, string destinationFolder)
        {
            if (Directory.Exists(destinationFolder))
            {
                // clear everything except the .git folder
                foreach (var d in Directory.GetDirectories(destinationFolder))
                {
                    if (Path.GetFileName(d) == ".git")
                        continue;
                    Directory.Delete(d, true);
                }
                foreach (var f in Directory.GetFiles(destinationFolder))
                {
                    File.Delete(f);
                }
            }
            else
                Directory.CreateDirectory(destinationFolder);

            ZipFile.ExtractToDirectory(file, destinationFolder);

            // strip timestamps
            HashSet<string> propertiesToStrip = new HashSet<string>() { "createdTimestamp", "modifiedTime", "structureModifiedTime", "refreshedTime" };
            var dataModelSchemaFile = Path.Combine(destinationFolder, "DataModelSchema");
            var jobj = JObject.Parse(File.ReadAllText(dataModelSchemaFile, unicode));
            jobj.Descendants()
                .OfType<JProperty>()
                .Where(jp => propertiesToStrip.Contains(jp.Name))
                .ToList().ForEach(jp => jp.Remove());
            var bytes = unicode.GetBytes(jobj.ToString());
            File.WriteAllBytes(dataModelSchemaFile, bytes);

            // remove security bindings
            File.Delete(Path.Combine(destinationFolder, "SecurityBindings"));
            var contentTypesXmlPath = Path.Combine(destinationFolder, "[Content_Types].xml");
            var doc = XDocument.Load(contentTypesXmlPath);
            doc
                .Descendants(XName.Get("Override", @"http://schemas.openxmlformats.org/package/2006/content-types"))
                .OfType<XElement>()
                .Where(xe => xe.Attribute("PartName")?.Value == "/SecurityBindings")
                .Single()
                .Remove();
            doc.Save(contentTypesXmlPath);

            // reformat json files
            Directory.GetFiles(destinationFolder, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(destinationFolder, "Report"), "*", SearchOption.AllDirectories))
                .ToList()
                .ForEach(f =>
                {
                    var bytes = File.ReadAllBytes(f);
                    var obj = ParseJsonStr(bytes);
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(f, formatted);
                });
        }

        private static JObject ParseJsonStr(byte [] bytes)
        {
            var encodingsToTry = new[] { unicode, Encoding.UTF8 };
            foreach (var enc in encodingsToTry)
            {
                try 
                {
                    return JObject.Parse(enc.GetString(bytes));
                }
                catch { }
            }
            throw new Exception("Unknown encoding or error in json string");
        }
    }
}