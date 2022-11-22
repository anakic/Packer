using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Stores.Folder.Transforms
{
    class UnstuffTransform : IJObjTransform
    {
        public void Restore(JObject obj)
        {
            foreach (var configJObj in obj.SelectTokens("..#config").ToArray())
                configJObj.Parent!.Replace(new JProperty("config", configJObj.ToString(Formatting.None)));

            foreach (var filtersJArr in obj.SelectTokens("..#filters").ToArray())
                filtersJArr.Parent!.Replace(new JProperty("filters", filtersJArr.ToString(Formatting.None)));
        }

        public void Transform(JObject obj)
        {
            var tokens = obj.SelectTokens("..config").Where(t => t?.Type == JTokenType.String).ToArray();
            foreach (var tok in tokens)
            {
                var jObj = JObject.Parse(tok.Value<string>()!);
                tok.Parent!.Replace(new JProperty("#config", jObj));
            }

            var filtersTokens = obj.SelectTokens("..filters").Where(t => t?.Type == JTokenType.String).ToArray();
            foreach (var tok in filtersTokens)
            {
                var jArr = JArray.Parse(tok.Value<string>()!).OfType<JObject>().ToArray();
                tok.Parent!.Replace(new JProperty("#filters", jArr));
            }
        }
    }
}
