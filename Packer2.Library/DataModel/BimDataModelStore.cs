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
            file.SetText(serialized);
        }
    }
}
