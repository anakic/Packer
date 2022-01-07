using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Packer // Note: actual namespace depends on the project name.
{
    class Program
    {
        static Encoding encoding = Encoding.Unicode;

        static void Main(string[] args)
        {
            Unpack(@"C:\TEST_PBI_VC\aw_sales.pbit", @"C:\TEST_PBI_VC\unpacked");
            Pack(@"C:\TEST_PBI_VC\unpacked", @"C:\TEST_PBI_VC\aw_sales_NEW.pbit");

            return;

            var operation = args[0];
            if (operation == "pack")
                Pack(args[1], args[2]);

            else if (operation == "unpack")
                Unpack(args[1], args[2]);
        }
        static void Pack(string sourceFolder, string destinationFilePath)
        {
            Debugger.Launch();
            var files = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                byte[] buffer = new byte[2];

                using (var fs = File.OpenRead(file))
                    fs.Read(buffer);

                if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    var bytes = File.ReadAllBytes(file);
                    File.WriteAllBytes(file, bytes.Skip(2).ToArray());
                }
            }

            Directory.GetFiles(sourceFolder, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(sourceFolder, "Report"), "*", SearchOption.AllDirectories))
                .ToList()
                .ForEach(f =>
                {
                    var encodingToUse = string.Equals(Path.GetExtension(f), ".json", StringComparison.OrdinalIgnoreCase) ? Encoding.UTF8 : encoding;
                    var str = File.ReadAllText(f);
                    var obj = JObject.Parse(str);
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.None);
                    var bytes = encoding.GetBytes(formatted);
                    File.WriteAllBytes(f, bytes);
                });

            // ZipFile.CreateFromDirectory(sourceFolder, destinationFilePath + ".a.pbit");
            ZipHelper.CreateFromDirectory(sourceFolder, destinationFilePath, CompressionLevel.Optimal, true, f => !f.Contains(".git"));
        }

        public static class ZipHelper
        {
            public static void CreateFromDirectory(
                string sourceDirectoryName
                , string destinationArchiveFileName
                , CompressionLevel compressionLevel
                , bool includeBaseDirectory
                , Predicate<string> filter // Add this parameter
            )
            {
                if (string.IsNullOrEmpty(sourceDirectoryName))
                {
                    throw new ArgumentNullException("sourceDirectoryName");
                }
                if (string.IsNullOrEmpty(destinationArchiveFileName))
                {
                    throw new ArgumentNullException("destinationArchiveFileName");
                }
                var filesToAdd = Directory.GetFiles(sourceDirectoryName, "*", SearchOption.AllDirectories);
                using (var zipFileStream = new FileStream(destinationArchiveFileName, FileMode.Create))
                {
                    using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
                    {
                        for (int i = 0; i < filesToAdd.Length; i++)
                        {
                            // Add the following condition to do filtering:
                            if (!filter(filesToAdd[i]))
                            {
                                continue;
                            }
                            var filePath = filesToAdd[i];
                            var entry = archive.CreateEntryFromFile(filePath, filePath.Substring(sourceDirectoryName.Length + 1).Replace("\\", "/"), compressionLevel);
                        }
                    }
                }
            }
        }

        static void Unpack(string file, string destinationFolder)
        {
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
            Directory.CreateDirectory(destinationFolder);

            ZipFile.ExtractToDirectory(file, destinationFolder);

            // strip timestamps
            HashSet<string> propertiesToStrip = new HashSet<string>() { "createdTimestamp", "modifiedTime", "structureModifiedTime", "refreshedTime" };
            var dataModelSchemaFile = Path.Combine(destinationFolder, "DataModelSchema");
            var jobj = JObject.Parse(File.ReadAllText(dataModelSchemaFile, encoding));
            jobj.Descendants()
                .OfType<JProperty>()
                .Where(jp => propertiesToStrip.Contains(jp.Name))
                .ToList().ForEach(jp => jp.Remove());
            var bytes = encoding.GetBytes(jobj.ToString());
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

            //// reformat json files
            //Directory.GetFiles(destinationFolder, "*")
            //    .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
            //    .Union(Directory.GetFiles(Path.Combine(destinationFolder, "Report"), "*", SearchOption.AllDirectories))
            //    .ToList()
            //    .ForEach(f =>
            //    {
            //        var encodingToUse = string.Equals(Path.GetExtension(f), ".json", StringComparison.OrdinalIgnoreCase) ? Encoding.UTF8 : encoding;
            //        var str = File.ReadAllText(f, encodingToUse);
            //        var obj = JObject.Parse(str);
            //        var formatted = obj.ToString(Newtonsoft.Json.Formatting.Indented);
            //        File.WriteAllText(f, formatted);
            //    });
        }
    }
}