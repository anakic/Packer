using System.Text;

namespace Packer.Model
{
    class TextFileItem : FileSystemItem
    {
        public TextFileItem(string path, string text) 
            : base(path)
        {
            Text = text;
        }

        public string Text { get; set; }

        internal override byte[] GetBytesToSave()
        {
            return Encoding.Unicode.GetBytes(Text);
        }
    }
}
