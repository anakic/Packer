using Newtonsoft.Json.Linq;
using System.Text;

namespace Packer.Steps
{
    internal class ExtractTablesStep : StepBase
    {
        public override void Extract(string pbitFilePath, string folderPath)
        {
            // strip timestamps
            var dataModelSchemaFile = Path.Combine(folderPath, "DataModelSchema");
            var jObj = JObject.Parse(File.ReadAllText(dataModelSchemaFile, Encoding.Unicode));
            var tableJObjects = jObj.Descendants()
                .OfType<JProperty>()
                .Single(jp => jp.Name == "tables")
                .Children().Cast<JArray>().Single()
                .Children<JObject>()
                .ToArray();

            if (tableJObjects.Any())
            {
                string tablesDir = Path.Combine(folderPath, "Tables");
                if (!Directory.Exists(tablesDir))
                    Directory.CreateDirectory(tablesDir);

                foreach (var tableJObject in tableJObjects)
                {
                    var tableName = tableJObject["name"]!.Value<string>()!;
                    string tableSchemaFilePath = Path.Combine(tablesDir, tableName);
                    File.WriteAllText(tableSchemaFilePath, tableJObject.ToString(Newtonsoft.Json.Formatting.Indented));
                    tableJObject.Replace(new JObject(){ new JProperty("$fileRef", Path.Combine("Tables", tableName)) });
                }

                File.WriteAllText(dataModelSchemaFile, jObj.ToString(Newtonsoft.Json.Formatting.Indented));
            }

            base.Extract(pbitFilePath, folderPath);
        }

        public override void Pack(string folderPath, string pbitFilePath)
        {
            base.Pack(folderPath, pbitFilePath);
        }
    }
}
