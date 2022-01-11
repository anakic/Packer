using Newtonsoft.Json.Linq;
using Packer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.Steps
{
    abstract class JsonFilesStep : StepBase
    {
        protected abstract string ExtractJson(JObject jObject);
        protected abstract string PackJson(JObject jObject);


        public override void Extract(string pbitFilePath, string folderPath)
        {
            Directory.GetFiles(folderPath, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(folderPath, "Report"), "*", SearchOption.AllDirectories))
                .ToList()
                .ForEach(f =>
                {
                    var allBytes = File.ReadAllBytes(f);
                    var obj = JsonFileHelpers.ParseJsonStr(allBytes);
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
                    var allBytes = File.ReadAllBytes(f);
                    var obj = JsonFileHelpers.ParseJsonStr(allBytes);
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.Indented);
                    File.WriteAllText(f, formatted);
                });
        }
    }
}
