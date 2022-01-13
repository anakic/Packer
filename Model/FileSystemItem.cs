using Packer.Storage;

namespace Packer.Model
{
    abstract class FileSystemItem
    {
        public string Path { get; }

        public FileSystemItem(string path)
        {
            Path = path;
        }

        internal abstract void SaveForMachine(IFilesStore store);
        internal abstract void SaveForHuman(IFilesStore store);
    }
}
