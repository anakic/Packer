using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    /// <summary>
    /// Extracts the pages from the Layout json file into a Pages subfolder. This makes it easier to edit and version individual pages.
    /// </summary>
    internal class ExtractBookmarksStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            var pageJObjects = model.LayoutFile!.JObj.SelectTokens("#config.bookmarks[*]").OfType<JObject>();

            if (pageJObjects.Any())
            {
                var bookmarkObjects = new List<JObject>();
                foreach (var pageJObj in pageJObjects)
                {
                    var childrenToken = pageJObj["children"];
                    if (childrenToken == null)
                        bookmarkObjects.Add(pageJObj);
                    else
                        bookmarkObjects.AddRange((childrenToken as JArray)!.Children().Cast<JObject>());
                }

                foreach (var pageJObj in bookmarkObjects)
                {
                    var pageName = pageJObj["displayName"]!.Value<string>()!;
                    var pageFileItem = model.AddExtractedBookmarkFile(pageName, pageJObj);
                    pageJObj.Replace(new JObject() { new JProperty("$fileRef", pageFileItem.Path) });
                }

            }
            base.ToHumanReadable(model);
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            base.ToMachineReadable(model);
            model.ClearExtractedBookmarks();
        }
    }
}
