using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    internal class ExtractPagesStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            var pageJObjects = model.LayoutFile!.JObj
                .Descendants()
                .OfType<JProperty>()
                .Single(jp => jp.Name == "sections")
                .Children().Cast<JArray>().Single()
                .Children<JObject>()
                .ToArray();

            if (pageJObjects.Any())
            {
                foreach (var pageJObj in pageJObjects)
                {
                    var pageName = pageJObj["displayName"]!.Value<string>()!;
                    var pageFileItem = model.AddExtractedPageFile(pageName, pageJObj);
                    pageJObj.Replace(new JObject() { new JProperty("$fileRef", pageFileItem.Path) });
                }
            }

            base.ToHumanReadable(model);
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            base.ToMachineReadable(model);
            model.ClearExtractedPages();
        }
    }
}
