namespace Packer2.Library
{
    public class MemoryFile : ITextFile
    {
        public string? Text { get; private set; }

        public MemoryFile(string? text = null)
        {
            Text = text;
        }

        public string GetText() => Text;

        public void SetText(string text) => Text = text;
    }
}
