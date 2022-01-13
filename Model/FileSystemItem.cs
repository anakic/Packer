﻿namespace Packer.Model
{
    abstract class FileSystemItem
    {
        public string Path { get; }

        public FileSystemItem(string path)
        {
            Path = path;
        }

        internal abstract byte [] GetBytesToSave();
    }
}