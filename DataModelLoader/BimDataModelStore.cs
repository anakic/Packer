using Microsoft.AnalysisServices.Tabular;
using Packer;

namespace DataModelLoader
{
    public class BimDataModelStore : DataModelStore
    {
        private readonly ITextFile file;

        public static BimDataModelStore LoadFromText(string contents)
        {
            return new BimDataModelStore(new InMemoryFile(contents));
        }
        public static BimDataModelStore LoadFromFile(string filePath)
        {
            return new BimDataModelStore(new TextBasedFile(filePath));
        }

        public BimDataModelStore(ITextFile file)
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
            file.SetText(serialized);
        }
    }
}
