namespace Packer2.FileSystem
{
   public sealed class LocalFileSystem : IFileSystem
    {
        public const string StoreKind = "FileStore";

        private readonly string basePath;

        public LocalFileSystem(string basePath)
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            this.basePath = basePath;
        }

        private bool IsFolder(string absPath)
            => string.IsNullOrEmpty(Path.GetExtension(absPath));

        public string Kind => StoreKind;

        public IPathResolver PathResolver => DefaultPathResolver.Instance;

        public void DeleteFile(string filePath)
        {
            string path = ToAbsolutePath(filePath);
            if (File.Exists(path))
                File.Delete(path);
        }

        public DateTime GetLastChangedTime(string filePath)
            => new FileInfo(ToAbsolutePath(filePath)).LastWriteTime;

        public string ReadAsString(string filePath)
        {
            using (var streamReader = new StreamReader(OpenStream(ToAbsolutePath(filePath), FileMode.Open, FileAccess.Read, FileShare.Read)))
                return streamReader.ReadToEnd();
        }

        public byte[] ReadAsBytes(string filePath)
        {
            using (var streamReader = new BinaryReader(OpenStream(ToAbsolutePath(filePath), FileMode.Open, FileAccess.Read, FileShare.Read)))
                return streamReader.ReadBytes((int)streamReader.BaseStream.Length);
        }

        public void Save(string filePath, byte[] content)
            => DoWrite(filePath, content, SaveBytes);

        public void Save(string filePath, string content)
            => DoWrite(filePath, content, SaveText);

        private void DoWrite<T>(string filePath, T content, Action<string, T> writeAction)
        {
            var filePathAbs = ToAbsolutePath(filePath);
            var folderPathAbs = Path.GetDirectoryName(filePathAbs);
            if (!Directory.Exists(folderPathAbs))
                Directory.CreateDirectory(folderPathAbs);
            writeAction(filePathAbs, content);
        }
        public bool FileExists(string filePath)
            => File.Exists(ToAbsolutePath(filePath));

        public IEnumerable<string> GetFilesRecursive(string folderPath)
            => GetFiles(folderPath, SearchOption.AllDirectories);

        public IEnumerable<string> GetFiles(string folderPath)
            => GetFiles(folderPath, SearchOption.TopDirectoryOnly);

        IEnumerable<string> GetFiles(string folderPath, SearchOption searchOption)
        {
            var folderPathAbs = ToAbsolutePath(folderPath);
            if (Directory.Exists(folderPathAbs))
            {
                DirectoryInfo directory = new DirectoryInfo(folderPathAbs);
                FileInfo[] files = directory.GetFiles("*.*", searchOption);
                return files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).Select(fi => fi.FullName).Select(ToRelativePath);
            }
            else
                return Array.Empty<string>();
        }

        public IEnumerable<string> GetFolders(string folderPath)
            => GetFolders(folderPath, SearchOption.TopDirectoryOnly);

        public IEnumerable<string> GetFoldersRecursive(string folderPath)
            => GetFolders(folderPath, SearchOption.AllDirectories);

        private IEnumerable<string> GetFolders(string folderPath, SearchOption searchOption)
        {
            var folderPathAbs = ToAbsolutePath(folderPath);
            if (Directory.Exists(folderPathAbs))
            {
                DirectoryInfo directory = new DirectoryInfo(folderPathAbs);
                DirectoryInfo[] directories = directory.GetDirectories("*", searchOption);
                return directories.Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden)).Select(fi => fi.FullName).Select(ToRelativePath);
            }
            else
                return Array.Empty<string>();
        }

        public void MoveFile(string path, string newPath)
        {
            File.Move(ToAbsolutePath(path), ToAbsolutePath(newPath));
        }

        public void DeleteFolder(string folderPath)
        {
            var folderPathAbs = ToAbsolutePath(folderPath);
            if (Directory.Exists(folderPathAbs))
            {
                // without this, if files and folder were marked as readonly, delete would fail
                SetAttributesNormal(new DirectoryInfo(folderPathAbs));
                Directory.Delete(folderPathAbs, true);
            }
        }

        private void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                SetAttributesNormal(subDir);
                subDir.Attributes = FileAttributes.Normal;
            }
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }


        public void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(ToAbsolutePath(folderPath));
        }

        public void MoveFolder(string originalPath, string newPath)
        {
            Directory.Move(ToAbsolutePath(originalPath), ToAbsolutePath(newPath));
        }

        public bool FolderExists(string path)
            => Directory.Exists(ToAbsolutePath(path));

        public IFileSystem Sub(string childFolderPath)
            => new LocalFileSystem(Path.Combine(basePath, childFolderPath));

        string ToRelativePath(string absPath)
            => PathResolver.GetRelativePath(absPath, basePath);

        string ToAbsolutePath(string relPath)
            => PathResolver.CombinePath(basePath, relPath);
        private void SaveBytes(string path, byte[] data)
        {
            using (var stream = OpenStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                stream.Write(data, 0, data.Length);
        }

        private void SaveText(string path, string text)
        {
            using (var streamWriter = new StreamWriter(OpenStream(path, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                // text has a mix of \n and \r\n across files, we want to normalize to \r\n
                if (!string.IsNullOrEmpty(text))
                {
                    text = text.Replace("\r\n", "\n").Replace("\n", "\r\n");
                }
                streamWriter.Write(text);
            }
        }

        private Stream OpenStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare share)
        {
            return File.Open(path, fileMode, fileAccess, share);
        }
    }
}