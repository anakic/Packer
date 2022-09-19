using System.Text;

namespace DataModelLoader
{
    // some pbi files (DataModelSchema) do not have a BOM header
    // so the encoding cannot be determined automatically
    public class LocalUnicodeFileWithoutBOM : ITextFile
    {
        private readonly string path;

        public LocalUnicodeFileWithoutBOM(string path)
        {
            this.path = path;
        }

        public string GetText() => System.IO.File.ReadAllText(path, Encoding.Unicode);

        public void SetText(string text) => System.IO.File.WriteAllBytes(path, Encoding.Unicode.GetBytes(text));
    }
}
