namespace Packer.Model
{
    class BinaryFileItem : FileSystemItem
    {
        public BinaryFileItem(string path, byte[] bytes) 
            : base(path)
        {
            Bytes = bytes;
        }

        public byte[] Bytes { get; set; }

        internal override byte[] GetBytesToSave()
            => Bytes;
    }
}
