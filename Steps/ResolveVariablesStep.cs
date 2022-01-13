using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    internal class ResolveVariablesStep : StepBase
    {
        public override void Pack(RepositoryModel model)
        {
            base.Pack(model);

            model.GetAllJsonFiles()
                .ToList()
                .ForEach(jf =>
                {
                    var fileRefObjects = jf.JObj.Descendants().OfType<JObject>()
                        .Select(obj => new { obj, fileRef = obj.Property("$fileRef")?.Value.ToString() })
                        .Where(x => x.fileRef != null)
                        .ToList();

                    foreach (var fileRefObj in fileRefObjects)
                    {
                        var resolved = model.GetExtractedJsonFile(fileRefObj.fileRef!);
                        fileRefObj.obj.Replace(resolved.JObj);
                    }
                });
        }
    }
}
