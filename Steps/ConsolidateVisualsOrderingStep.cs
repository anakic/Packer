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
                Consolidate(pageFile, "tabOrder");
                Consolidate(pageFile, "z");
            }
        }

        private static void Consolidate(JsonFileItem pageFile, string property)
        {
            Dictionary<string, int> visualsOrder = new Dictionary<string, int>();
            foreach (var container in pageFile.JObj.SelectTokens("visualContainers[*]"))
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
            pageFile.JObj.Add(new JProperty($"#{property}", visualsOrderArr));
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            throw new NotImplementedException();
            // procitati order listu i za svaki item:
            // - uzeti da mu je tab order = index_u_listi * 100
            // - naci #config item kojem pripada
            // - upisati tabOrder u #config/layout[0] jobj
            // - upisati tabOrder u parent #config jobjecta
        }
    }
}
