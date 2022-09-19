using DataModelLoader;
using System.IO;

namespace Packer
{
    public class InMemoryFile : ITextFile
    {
        public string Text { get; private set; }

        public InMemoryFile(string text = null)
        {
            Text = text;    
        }

        public string GetText() => Text;

        public void SetText(string text) => Text = text;
    }

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
