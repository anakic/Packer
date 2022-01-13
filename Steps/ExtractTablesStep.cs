using Newtonsoft.Json.Linq;
using Packer.Model;
using System.Text;

namespace Packer.Steps
{
    internal class ExtractTablesStep : StepBase
    {
        public override void Extract(RepositoryModel model)
        {
            model.DataModelSchemaFile.Modify(jObj => 
            {
                var tableJObjects = jObj
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
            });

            base.Extract(model);
        }

        public override void Pack(RepositoryModel model)
        {
            // [1. resolve packed variables (we don't need to do that, we can just add the resolve variables step to the beginning and make sure it runs first)]
            // delete "tables" folder

            base.Pack(model);
        }

    }
}
