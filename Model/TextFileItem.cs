using Packer.Storage;
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

        internal override void SaveForMachine(IFilesStore store)
            => SaveForHuman(store);

        internal override void SaveForHuman(IFilesStore store)
        {
            store.Write(Path, Encoding.Unicode.GetBytes(Text));
        }
    }
}
