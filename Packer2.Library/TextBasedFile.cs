namespace Packer2.Library
{
    public class TextBasedFile : ITextFile
    {
        private readonly string path;

        public TextBasedFile(string path)
        {
            this.path = path;
        }

        public string GetText() => File.ReadAllText(path);

        public void SetText(string text) => File.WriteAllText(path, text);
    }
}
