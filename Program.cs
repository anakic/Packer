using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Packer
{
    internal static class Program
    {
        private static readonly Encoding Encoding = Encoding.Unicode;

        public static void Main(string[] args)
        {
            var operation = args[0];
            switch (operation)
            {
                case "pack":
                    Pack(args[1], args[2]);
                    break;
                case "unpack":
                    Unpack(args[1], args[2]);
                    break;
            }
        }

        private static void Pack(string sourceFolder, string outputFilePath)
        {
            var tempFolderPath = Path.Combine(Path.GetTempPath(), "Packer_temp");

            if (Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);

            foreach (var filePath in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
            {
                if (filePath.Contains(".git\\") || filePath.Contains("-schema.json"))
                    continue;

                var relativePath = filePath[(sourceFolder.TrimEnd('\\').Length + 1)..];
                var destinationPath = Path.Combine(tempFolderPath, relativePath);

                var destinationDir = Path.GetDirectoryName(destinationPath) ?? throw new Exception("Invalid path");
                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                File.Copy(filePath, destinationPath);
            }

            Directory.GetFiles(tempFolderPath, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(tempFolderPath, "Report"), "*", SearchOption.AllDirectories))
                .ToList()
                .ForEach(f =>
                {
                    var str = File.ReadAllText(f);
                    var obj = JObject.Parse(str);
                    obj.Descendants().OfType<JProperty>().Where(d=>d.Name == "$schema").ToList().ForEach(c => c.Remove());
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.None);
                    var bytes = Encoding.GetBytes(formatted);
                    File.WriteAllBytes(f, bytes);
                });

            if(File.Exists(outputFilePath))
                File.Delete(outputFilePath);
            ZipFile.CreateFromDirectory(tempFolderPath, outputFilePath);

            Directory.Delete(tempFolderPath, true);
        }

        private static void Unpack(string file, string destinationFolder)
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
            var propertiesToStrip = new HashSet<string>() { "createdTimestamp", "modifiedTime", "structureModifiedTime", "refreshedTime", "lastUpdate", "lastSchemaUpdate", "lastProcessed" };
            var dataModelSchemaFile = Path.Combine(destinationFolder, "DataModelSchema");
            var jObj = JObject.Parse(File.ReadAllText(dataModelSchemaFile, Encoding));
            jObj.Descendants()
                .OfType<JProperty>()
                .Where(jp => propertiesToStrip.Contains(jp.Name))
                .ToList().ForEach(jp => jp.Remove());
            var bytes = Encoding.GetBytes(jObj.ToString());
            File.WriteAllBytes(dataModelSchemaFile, bytes);

            // remove security bindings
            File.Delete(Path.Combine(destinationFolder, "SecurityBindings"));
            var contentTypesXmlPath = Path.Combine(destinationFolder, "[Content_Types].xml");
            var doc = XDocument.Load(contentTypesXmlPath);
            doc
                .Descendants(XName.Get("Override", @"http://schemas.openxmlformats.org/package/2006/content-types"))
                .Single(xe => xe.Attribute("PartName")?.Value == "/SecurityBindings")
                .Remove();
            doc.Save(contentTypesXmlPath);

            // reformat json files
            Directory.GetFiles(destinationFolder, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(destinationFolder, "Report"), "*", SearchOption.AllDirectories))
                .ToList()
                .ForEach(f =>
                {
                    var allBytes = File.ReadAllBytes(f);
                    var obj = ParseJsonStr(allBytes);
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(f, formatted);
                });
        }

        private static JObject ParseJsonStr(byte [] bytes)
        {
            var encodingsToTry = new[] { Encoding, Encoding.UTF8 };
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
}