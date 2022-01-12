using Newtonsoft.Json.Linq;

namespace Packer.Steps
{
    internal class ResolveVariablesStep : StepBase
    {
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

                    var fileRefObjects = obj.Descendants().OfType<JObject>()
                        .Select(obj => new { obj, fileRef = obj.Property("$fileRef")?.Value.ToString() })
                        .Where(x => x.fileRef != null)
                        .ToList();

                    foreach (var fileRefObj in fileRefObjects)
                    {
                        var fileText = File.ReadAllText(Path.Combine(folderPath, fileRefObj.fileRef!));
                        var resolvedContent = JObject.Parse(fileText);
                        fileRefObj.obj.Replace(resolvedContent);
                    }
                        
                    var formatted = obj.ToString(Newtonsoft.Json.Formatting.None);
                    File.WriteAllText(f, formatted);
                });

            // todo: remove variable folders (for now just Tables, but in the future possibly ConnectionStrings and others?)
        }
    }
}
