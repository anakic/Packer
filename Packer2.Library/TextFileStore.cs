namespace Packer2.Library
{
    public class TextFileStore : ITextStore
    {
        private readonly string path;

        public TextFileStore(string path)
        {
            this.path = path;
        }

        public string GetText() => File.ReadAllText(path);

        public void SetText(string text)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(path, text);
        }
    }
}
