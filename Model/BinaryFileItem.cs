using Packer.Storage;

namespace Packer.Model
{
    /// <summary>
    /// Represents a binary file in the repository.
    /// </summary>
    class BinaryFileItem : FileSystemItem
    {
        public BinaryFileItem(string path, byte[] bytes) 
            : base(path)
        {
            Bytes = bytes;
        }

        public byte[] Bytes { get; set; }

        internal override void SaveForMachine(IFilesStore store)
            => store.Write(Path, Bytes);
        internal override void SaveForHuman(IFilesStore store)
            => SaveForMachine(store);
    }
}
