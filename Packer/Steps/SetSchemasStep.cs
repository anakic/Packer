using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Packer.Model;
using System.Text;

namespace Packer.Steps
{
    /// <summary>
    /// Schema files are used for two thigs:
    /// 1. they are copied into the repo and a $schema property is set on the targetted json files so that json editors(e.g.VSCode) will offer error checking when editing the json manually.
    /// 2. targetted json files are validated against corresponding schemas when packing; any validation errors are displayed to the user as warnings in the console.
    /// </summary>
    internal class SetSchemasStep : StepBase
    {
        private ILogger<SetSchemasStep> logger;

        public SetSchemasStep(ILogger<SetSchemasStep> logger)
        {
            this.logger = logger;
        }

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
                    var text = new StringBuilder();
                    text.AppendLine($"File '{file.Path}' failed json validation with the following messages: ");
                    foreach (var m in messages)
                        text.AppendLine($"({m.LineNumber}, {m.LinePosition}) {m.Message}");
                    logger.LogWarning(text.ToString());
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
