using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Stores.Folder.Transforms
{
    class UnstuffTransform : IJObjTransform
    {
        string[] selectorsObj = new[] { "..config", "sections[*].visualContainers[*].query", "sections[*].visualContainers[*].dataTransforms" };
        string[] selectorsArr = new[] { "pods[*].parameters", "..filters" };

        public void Restore(JObject obj)
        {
            foreach (string selector in selectorsObj.Union(selectorsArr))
                Restore(obj, selector);
        }

        public void Transform(JObject obj)
        {
            foreach (string selector in selectorsObj)
                TransformObj(obj, selector);

            foreach (string selector in selectorsArr)
                TransformArr(obj, selector);
        }

        private void Restore(JObject obj, string path)
        {
            var pathAdjusted = path.Insert(path.LastIndexOf(".") + 1, "#");

            foreach (var filtersJArr in obj.SelectTokens(pathAdjusted).ToArray())
                filtersJArr.Parent!.Replace(new JProperty(pathAdjusted.Substring(1 + pathAdjusted.LastIndexOf("#")), filtersJArr.ToString(Formatting.None)));
        }

        private void TransformObj(JObject obj, string path)
        {
            var tokens = obj.SelectTokens(path).Where(t => t?.Type == JTokenType.String).ToArray();
            foreach (var tok in tokens)
            {
                var jObj = JObject.Parse(tok.Value<string>()!);
                tok.Parent!.Replace(new JProperty("#" + path.Substring(path.LastIndexOf('.') + 1), jObj));
            }
        }

        private void TransformArr(JObject obj, string path)
        {
            var tokens = obj.SelectTokens(path).Where(t => t?.Type == JTokenType.String).ToArray();
            foreach (var tok in tokens)
            {
                var jArr = JArray.Parse(tok.Value<string>()!);
                tok.Parent!.Replace(new JProperty("#" + path.Substring(path.LastIndexOf('.') + 1), jArr));
            }
        }
    }
}
