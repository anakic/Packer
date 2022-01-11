using Newtonsoft.Json.Linq;
using Packer.Tools;
using System.Text;

namespace Packer.Steps
{
    internal class ReformatJsonFilesStep : StepBase
    {
        public override void Extract(string pbitFilePath, string folderPath)
        {
            Directory.GetFiles(folderPath, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(folderPath, "Report"), "*", SearchOption.AllDirectories))
                .ToList()
                .ForEach(f =>
                {
                    var obj = JObject.Parse(File.ReadAllText(f));
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(f, formatted);
                });

            base.Extract(pbitFilePath, folderPath);
        }

        public override void Pack(string folderPath, string pbitFilePath)
        {
            base.Pack(folderPath, pbitFilePath);

            Directory.GetFiles(folderPath, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(folderPath, "Report"), "*", SearchOption.AllDirectories))
                .ToList()
                .ForEach(f =>
                {
                    var str = File.ReadAllText(f);
                    var obj = JObject.Parse(str);
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.None);
                    File.WriteAllText(f, formatted);
                });
        }
    }
}
