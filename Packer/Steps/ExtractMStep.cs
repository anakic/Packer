using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    /// <summary>
    /// Extracts the tables from the DataModelSchema json file into a Pages subfolder. This makes it easier to edit and version individual tables of the model.
    /// </summary>
    internal class ExtractMStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            foreach (var tableFile in model.ExtractedTableFiles)
            {
                foreach (var partitionToken in tableFile.JObj.SelectTokens("partitions[*]"))
                {
                    var expressionTypeToken = partitionToken.SelectToken("source.type") as JValue;
                    if (expressionTypeToken!.Value<string>() != "m")
                        continue;

                    var expressionToken = partitionToken.SelectToken("source.expression");
                    if (expressionToken == null)
                        continue;

                    var expression = expressionToken.Value<string>()!.Trim();
                    string tableName = tableFile.JObj["name"]!.Value<string>()!;
                    string partitionName = partitionToken["name"]!.Value<string>()!;
                    var file = model.AddExtractedMFile(tableName, partitionName, expression);
                    expressionToken.Replace(new JObject(new JProperty("$fileStringRef", file.Path)));
                }
            }
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            model.ClearExtractedMFiles();
        }
    }
}
