using Packer.Storage;
using System.Text;

namespace Packer.Model
{
    /// <summary>
    /// Represents a text file in the repository. Allows getting/setting
    /// the text of the file.
    /// </summary>
    /// <remarks>
    /// Currently, used only for the Version file
    /// </remarks>
    class TextFileItem : FileSystemItem
    {
        public TextFileItem(string path, string text) 
            : base(path)
        {
            Text = text;
        }

        public string Text { get; set; }

        internal override void SaveForMachine(IFilesStore store)
        {
            SaveForHuman(store);
        }

        internal override void SaveForHuman(IFilesStore store)
        {
            store.Write(Path, Text);
        }
    }
}
