using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    /// <summary>
    /// Stripts timestamps from the DataModelSchema file. These change when edited but they are not useful code
    /// and would pollute the diff with information that is not relevant to us. PowerBI also doesn't mind if we
    /// strip them out.
    /// </summary>
    internal class ConsolidateVisualsOrderingStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            foreach (var pageFile in model.ExtractedPageFiles)
            {
                UnstuffJsonStep.Unstuff(pageFile.JObj, "visualContainers[*].config");
                Consolidate(pageFile.JObj, "tabOrder");
                Consolidate(pageFile.JObj, "z");
                UnstuffJsonStep.Stuff(pageFile.JObj, "visualContainers[*].#config");
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
                container.SelectToken($"#config.layouts[0].position.{property}")!.Parent!.Remove();
                var visualName = container.SelectToken("#config.name")!.Value<string>()!;
                visualsOrder[visualName] = value;
            }
            var visualsOrderArr = visualsOrder.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToArray();
            pageFileJObj.Add(new JProperty($"#{property}", visualsOrderArr));
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            foreach (var pageFile in model.ExtractedPageFiles)
            {
                UnstuffJsonStep.Unstuff(pageFile.JObj, "visualContainers[*].config");
                Restore(pageFile.JObj, "tabOrder");
                Restore(pageFile.JObj, "z");
                UnstuffJsonStep.Stuff(pageFile.JObj, "visualContainers[*].#config");
            }
        }

        private static void Restore(JObject pageFileJObj, string property)
        {
            var arrToken = (pageFileJObj.SelectToken($"#{property}") as JArray)!;
            var tabOrderArr = arrToken.Values<string>()!.ToArray();

            var dict = pageFileJObj
                .SelectTokens("visualContainers[*]")!
                .ToDictionary(tok => tok.SelectToken("#config.name")!.Value<string>()!, tok => (JObject)tok);

            int order = 1;
            foreach (var visualName in tabOrderArr)
            {
                var value = 100 * order++;
                var visualToken = dict[visualName!];
                visualToken.Add(new JProperty(property, value));
                (visualToken.SelectToken($"#config.layouts[0].position") as JObject)!.Add(new JProperty(property, value));
            }
            ((JProperty)arrToken.Parent!).Remove();
        }
    }
}
