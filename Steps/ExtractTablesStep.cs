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
            var tableJObjects = model.DataModelSchemaFile!.JObj
                .Descendants()
                .OfType<JProperty>()
                .Single(jp => jp.Name == "tables")
                .Children().Cast<JArray>().Single()
                .Children<JObject>()
                .ToArray();

            if (tableJObjects.Any())
            {
                foreach (var tableJObject in tableJObjects)
                {
                    var tableName = tableJObject["name"]!.Value<string>()!;
                    var tableFileItem = model.AddExtractedTableFile(tableName, tableJObject);
                    tableJObject.Replace(new JObject() { new JProperty("$fileRef", tableFileItem.Path) });
                }
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
