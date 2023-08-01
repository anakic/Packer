using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel
{
    public class BimDataModelStore : IModelStore<Database>
    {
        private readonly ITextStore file;

        public static BimDataModelStore LoadFromText(string contents)
        {
            return new BimDataModelStore(new MemoryFile(contents));
        }
        public static BimDataModelStore LoadFromFile(string filePath)
        {
            return new BimDataModelStore(new TextFileStore(filePath));
        }

        public BimDataModelStore(ITextStore file)
        {
            this.file = file;
        }

        public Database Read()
        {
            return JsonSerializer.DeserializeDatabase(file.GetText());
        }

        public void Save(Database model)
        {
            var serialized = JsonSerializer.SerializeDatabase(model, new SerializeOptions() { IgnoreInferredObjects = true, IgnoreInferredProperties = true, IgnoreTimestamps = true });
            
            // todo: sort table names to avoid diff caused by ordering
            // I don't think we can sort the tables list directly, but we can sort the JSON array:
            // - load the json into a jobject
            // - find the tables jarray
            // - sort it by name
            // - replace the original jarray with the sorted one

            file.SetText(serialized);
        }
    }
}
