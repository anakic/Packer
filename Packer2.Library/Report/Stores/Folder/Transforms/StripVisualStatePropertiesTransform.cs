using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Stores.Folder.Transforms
{
    class StripVisualStatePropertiesTransform : IJObjTransform
    {
        public void Restore(JObject obj)
        {
        }

        public void Transform(JObject obj)
        {
            var propertiesToRemove = new string[] { "query", "dataTransforms" };
            foreach (var t in obj.SelectTokens(".sections[*].visualContainers[*]"))
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
