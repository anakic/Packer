using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    /// <summary>
    /// Extracts the tables from the DataModelSchema json file into a Pages subfolder. This makes it easier to edit and version individual tables of the model.
    /// </summary>
    internal class ExtractDaxStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            foreach (var tableFile in model.ExtractedTableFiles)
            {
                foreach (var measureToken in tableFile.JObj.SelectTokens("measures[*]"))
                {
                    var expressionToken = measureToken.SelectToken("expression");
                    if (expressionToken == null)
                        continue;

                    var expression = expressionToken.Value<string>()!.Trim();
                    string tableName = tableFile.JObj["name"]!.Value<string>()!;
                    string measureName = measureToken["name"]!.Value<string>()!;
                    var file = model.AddExtractedDaxFile(tableName, measureName, expression);
                    expressionToken.Replace(new JObject(new JProperty("$fileStringRef", file.Path)));
                }
            }
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            throw new NotImplementedException();
        }
    }
}
