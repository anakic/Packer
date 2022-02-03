using Packer.Model;
using System.Diagnostics;

namespace Packer.Steps
{
    internal class StripPageGenPropsSteps : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            string path = "visualContainers[*]";
            var propertiesToRemove = new string[] { "query", "dataTransforms" };
            foreach (var pageFile in model.ExtractedPageFiles)
            {
                foreach (var t in pageFile.JObj.SelectTokens(path))
                {
                    foreach (var prop in propertiesToRemove)
                    {
                        var tok = t.SelectToken(prop);
                        if (tok != null)
                            tok.Parent!.Remove();
                    }
                }
            }
        }
    }
}
