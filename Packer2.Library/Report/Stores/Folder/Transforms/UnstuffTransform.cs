using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Stores.Folder.Transforms
{
    class UnstuffTransform : IJObjTransform
    {
        string[] selectorsObj = new[] { "..config", "sections[*].visualContainers[*].query", "sections[*].visualContainers[*].dataTransforms" };
        string[] selectorsArr = new[] { "pods[*].parameters", "..filters" };
        private ILogger logger;

        public UnstuffTransform(ILogger logger)
        {
            this.logger = logger;
        }

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

        private void Restore(JObject obj, string selector)
        {
            var pathAdjusted = selector.Insert(selector.LastIndexOf(".") + 1, "#");

            foreach (var jObj in obj.SelectTokens(pathAdjusted).ToArray())
            {
                logger.LogInformation("Stuffing json object/array at path {path}", jObj.Path);
                jObj.Parent!.Replace(new JProperty(pathAdjusted.Substring(1 + pathAdjusted.LastIndexOf("#")), jObj.ToString(Formatting.None)));
            }
        }

        private void TransformObj(JObject obj, string selector)
        {
            var tokens = obj.SelectTokens(selector).Where(t => t?.Type == JTokenType.String).ToArray();
            foreach (var tok in tokens)
            {
                logger.LogInformation("Un-stuffing json object at path {path}", tok.Path);
                var jObj = JObject.Parse(tok.Value<string>()!);
                tok.Parent!.Replace(new JProperty("#" + selector.Substring(selector.LastIndexOf('.') + 1), jObj));
            }
        }

        private void TransformArr(JObject obj, string path)
        {
            var tokens = obj.SelectTokens(path).Where(t => t?.Type == JTokenType.String).ToArray();
            foreach (var tok in tokens)
            {
                logger.LogInformation("Un-stuffing json array at path {path}", tok.Path);
                var jArr = JArray.Parse(tok.Value<string>()!);
                tok.Parent!.Replace(new JProperty("#" + path.Substring(path.LastIndexOf('.') + 1), jArr));
            }
        }
    }
}
