using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Stores.Folder.Transforms
{
    class ConsolidateOrderingTransform : IJObjTransform
    {
        public void Restore(JObject obj)
        {
            foreach (var jo in obj.SelectTokens(".sections[*]").OfType<JObject>())
            {
                RestoreConsolidatedProperty(jo, "tabOrder");
                RestoreConsolidatedProperty(jo, "z");
            }
        }

        public void Transform(JObject obj)
        {
            foreach (var jo in obj.SelectTokens(".sections[*]").OfType<JObject>())
            {
                Consolidate(jo, "tabOrder");
                Consolidate(jo, "z");
            }
        }

        private static void Consolidate(JObject pageFileJObj, string property)
        {
            Dictionary<string, int> visualsOrder = new Dictionary<string, int>();
            foreach (var container in pageFileJObj.SelectTokens("visualContainers[*]"))
            {
                var toToken = container[property];
                if (toToken == null)
                    continue;

                var value = toToken.Value<int>();
                container.SelectToken(property)!.Parent!.Remove();

                var configObj = (JObject)container["#config"]!;
                configObj.SelectToken($".layouts[0].position.{property}")!.Parent!.Remove();
                var visualName = configObj["name"]!.Value<string>()!;
                visualsOrder[visualName] = value;
            }
            var visualsOrderArr = visualsOrder.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray();
            pageFileJObj.Add(new JProperty($"#{property}", visualsOrderArr));
        }

        private static void RestoreConsolidatedProperty(JObject pageFileJObj, string property)
        {
            var arrToken = (pageFileJObj.SelectToken($"#{property}") as JArray)!;
            var tabOrderArr = arrToken.Values<string>()!.ToArray();

            var dict = pageFileJObj.SelectTokens("visualContainers[*]")!
                .ToDictionary(vc => vc.SelectToken("#config.name")!.ToString(), kvp => (JObject)kvp);

            int order = 1;
            foreach (var visualName in tabOrderArr)
            {
                var value = 100 * order++;
                var visualToken = dict[visualName!];
                visualToken.Add(new JProperty(property, value));
                var configObj = (JObject)visualToken["#config"]!;
                (configObj.SelectToken($".layouts[0].position") as JObject)!.Add(new JProperty(property, value));
            }
            ((JProperty)arrToken.Parent!).Remove();
        }
    }
}