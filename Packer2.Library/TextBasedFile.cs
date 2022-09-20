namespace Packer2.Library
{
    public class LocalTextFile : ITextFile
    {
        private readonly string path;

        public LocalTextFile(string path)
        {
            this.path = path;
        }

        public string GetText() => File.ReadAllText(path);

        public void SetText(string text) => File.WriteAllText(path, text);
    }
}
