using Newtonsoft.Json.Linq;
using Packer.Storage;
using System.Reflection;

namespace Packer.Model
{
    public class RepositoryModelBase
    {
        protected JsonFileItem? ReadJson(IFilesStore fileSystem, string path)
        {
            // json files can be with or without an extension. initially they 
            // are without an extension, but we add an extension for better VSCode support.
            // we want to be able to load them just the same, regardless of if they have
            // the extension or not.

            if (fileSystem.FileExists(path))
                return JsonFileItem.Read(path, fileSystem);
            else if (fileSystem.FileExists(path + ".json"))
                return JsonFileItem.Read(path + ".json", fileSystem);
            return null;
        }

        // todo: return JsonSchemaFile (exposes JSchema instead of JObject)
        protected JsonFileItem ReadJsonSchema(string schemaFileDestinationPath)
        {
            var schemaFileName = Path.GetFileName(schemaFileDestinationPath);
            var schemasSourceFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Schemas");
            var absPath = Path.Combine(schemasSourceFolder, schemaFileName);
            var jObj = JObject.Parse(File.ReadAllText(absPath));
            return new JsonFileItem(schemaFileDestinationPath, jObj, false);
        }
    }
}