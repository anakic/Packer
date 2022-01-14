using Packer.Storage;

namespace Packer.Model
{
    /// <summary>
    /// Base class for files in the repository. Files have a path (relative inside the repo)
    /// and must know how to save themselves in two ways: for human readability and machine readability.
    /// </summary>
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
