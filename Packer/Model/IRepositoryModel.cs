using Newtonsoft.Json.Linq;
using Packer.Storage;

namespace Packer.Model
{
    internal interface IDataRepositoryModel
    {
        JsonFileItem? DataModelSchemaFile { get; set; }

        IEnumerable<TextFileItem> ExtractedDaxFiles { get; }
        IEnumerable<TextFileItem> ExtractedMFiles { get; }
        IEnumerable<JsonFileItem> ExtractedTableFiles { get; }
        JsonFileItem TableSchemaFile { get; set; }

        TextFileItem AddExtractedDaxFile(string tableName, string measureName, string text);
        TextFileItem AddExtractedMFile(string tableName, string partitionName, string text);
        JsonFileItem AddExtractedTableFile(string tableName, JObject jObject);
        void ClearExtractedDaxFiles();
        void ClearExtractedMFiles();
        void ClearExtractedTables();

        void WriteTo(IFilesStore fileSystem, bool forHuman);
    }
}