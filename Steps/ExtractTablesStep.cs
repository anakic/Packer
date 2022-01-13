using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
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

        public override void Pack(RepositoryModel model)
        {
            base.Pack(model);

            model.ClearExtractedTables();
        }
    }
}
