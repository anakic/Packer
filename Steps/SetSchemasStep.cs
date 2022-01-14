using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Packer.Model;
using System.Reflection;
using System.Text;

namespace Packer.Steps
{
    internal class SetSchemasStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            var fileSchemas = GetFileSchemas(model);
            fileSchemas.ToList()
                .ForEach(f =>
                {
                    var file = f.Key;
                    var schemaFile = f.Value;
                    // the schema file is always in the same folder as the files that use it
                    var schemaFileName = Path.GetFileName(schemaFile.Path);
                    file.JObj.Add(new JProperty("$schema", schemaFileName));
                });

            base.ToHumanReadable(model);
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            foreach (var kvp in GetFileSchemas(model))
            {
                var file = kvp.Key;
                var schemaFile = kvp.Value;
                file.JObj.Descendants().OfType<JProperty>().Where(d => d.Name == "$schema").ToList().ForEach(c => c.Remove());

                // todo: the schema file should expose JSchema instead of JObject
                var schema = JSchema.Parse(schemaFile.JObj.ToString());
                var isValid = file.JObj.IsValid(schema, out IList<ValidationError> messages);

                if (!isValid)
                {
                    Console.WriteLine($"Warning - file '{file.Path}' failed json validation with the following messages: ");
                    foreach (var m in messages)
                        Console.Write($"({m.LineNumber}, {m.LinePosition}) {m.Message}");
                }
            }

            base.ToMachineReadable(model);
        }

        private static Dictionary<JsonFileItem, JsonFileItem> GetFileSchemas(RepositoryModel model)
        {
            var fileSchemas = new Dictionary<JsonFileItem, JsonFileItem>();
            foreach (var et in model.ExtractedTableFiles)
                fileSchemas.Add(et, model.TableSchemaFile);
            //foreach (var et in model.ExtractedPageFiles)
            //    fileSchemas.Add(et, "page-schema.json");
            //fileSchemas.Add(model.LayoutFile!, "layout-schema.json");
            //fileSchemas.Add(model.DataModelSchemaFile!, "datamodelschema-schema.json");
            //fileSchemas.Add(model.DiagramLayoutFile!, "diagramlayout-schema.json");
            //fileSchemas.Add(model.MetadataFile!, "metadata-schema.json");
            //fileSchemas.Add(model.SettingsFile!, "settings-schema.json");
            return fileSchemas;
        }

    }
}
