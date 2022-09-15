using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    /// <summary>
    /// Extracts the tables from the DataModelSchema json file into a Pages subfolder. This makes it easier to edit and version individual tables of the model.
    /// </summary>
    internal class ExtractTablesStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            if (model.DataModelSchemaFile == null)
                return;

            var arr = model.DataModelSchemaFile!.JObj.SelectToken("model.tables")!;

            var tableJObjects = arr
                .Children<JObject>()
                .ToArray();

            if (tableJObjects.Any())
            {
                Dictionary<string, string> tablePaths = new Dictionary<string, string>();
                foreach (var tableJObject in tableJObjects)
                {
                    var tableName = tableJObject["name"]!.Value<string>()!;
                    var tableFileItem = model.AddExtractedTableFile(tableName, tableJObject);
                    tablePaths.Add(tableName, tableFileItem.Path);
                }
                arr.Replace(new JArray(tablePaths.OrderBy(kvp => kvp.Key).Select(kvp => new JObject(new JProperty("$fileRef", kvp.Value)))));
            }

            base.ToHumanReadable(model);
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            base.ToMachineReadable(model);

            model.ClearExtractedTables();
        }
    }
}
