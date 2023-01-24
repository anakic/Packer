namespace Packer2.FileSystem
{
    public class RelativeFileSystem : IFileSystem, IDisposable
    {
        public string Kind => InnerStore.Kind;

        public RelativeFileSystem(IFileSystem innerStore, string rootPath)
        {
            InnerStore = innerStore;
            BasePath = rootPath;
        }

        public IFileSystem InnerStore { get; }

        public string BasePath { get; private set; }

        public void RebaseTo(string newBasePath)
        {
            if (InnerStore.FolderExists(newBasePath))
                throw new InvalidOperationException($"Cannot rebase to path {newBasePath}, folder already exists.");

            InnerStore.MoveFolder(BasePath, newBasePath);
            BasePath = newBasePath;
        }

        public IPathResolver PathResolver => InnerStore.PathResolver;

        public void CreateFolder(string folderPath)
        {
            InnerStore.CreateFolder(ToAbsolutePath(folderPath));
        }

        public void DeleteFile(string name)
        {
            InnerStore.DeleteFile(ToAbsolutePath(name));
        }

        public void DeleteFolder(string name)
        {
            InnerStore.DeleteFolder(ToAbsolutePath(name));
        }

        public bool FileExists(string filePath)
        {
            return InnerStore.FileExists(ToAbsolutePath(filePath));
        }

        public bool FolderExists(string path)
        {
            return InnerStore.FolderExists(ToAbsolutePath(path));
        }

        public string ReadAsString(string filePath)
        {
            return InnerStore.ReadAsString(ToAbsolutePath(filePath));
        }

        public DateTime GetLastChangedTime(string filePath)
        {
            return InnerStore.GetLastChangedTime(ToAbsolutePath(filePath));
        }

        public byte[] ReadAsBytes(string filePath)
        {
            return InnerStore.ReadAsBytes(ToAbsolutePath(filePath));
        }

        public IEnumerable<string> GetFiles(string folderPath)
        {
            return InnerStore.GetFiles(ToAbsolutePath(folderPath)).Select(f => ToRelativePath(f));
        }

        public IEnumerable<string> GetFilesRecursive(string folderPath)
        {
            return InnerStore.GetFilesRecursive(ToAbsolutePath(folderPath)).Select(f => ToRelativePath(f));
        }


        public IEnumerable<string> GetFoldersRecursive(string folderPath)
        {
            return InnerStore.GetFoldersRecursive(ToAbsolutePath(folderPath)).Select(f => ToRelativePath(f));
        }

        public IEnumerable<string> GetFolders(string folderPath)
        {
            return InnerStore.GetFolders(ToAbsolutePath(folderPath)).Select(f => ToRelativePath(f));
        }

        public void MoveFile(string path, string newPath)
        {
            InnerStore.MoveFile(ToAbsolutePath(path), ToAbsolutePath(newPath));
        }

        public void MoveFolder(string originalPath, string newPath)
        {
            InnerStore.MoveFolder(ToAbsolutePath(originalPath), ToAbsolutePath(newPath));
        }

        public void Save(string filePath, string content)
        {
            InnerStore.Save(ToAbsolutePath(filePath), content);
        }

        public void Save(string filePath, byte[] content)
        {
            InnerStore.Save(ToAbsolutePath(filePath), content);
        }

        public string ToRelativePath(string absPath)
        {
            return PathResolver.GetRelativePath(absPath, BasePath);
        }

        public string ToAbsolutePath(string relPath)
        {
            return PathResolver.CombinePath(BasePath, relPath);
        }

        // instead of nesting another relative data store on top of this one, create a new relative data store
        // aiming directly at the inner store but using a combined path (base+child folder)
        public IFileSystem Sub(string childFolderPath)
            => new RelativeFileSystem(InnerStore, PathResolver.CombinePath(BasePath, childFolderPath));

        public void Dispose()
        {
            if (InnerStore is IDisposable d)
                d.Dispose();
        }

        /// <summary>
        /// Goes through the InnerStore chain and returns the first store that is not a RelativeDataStore.
        /// </summary>
        /// <returns></returns>
        public IFileSystem Unwrap()
        {
            if (InnerStore is RelativeFileSystem r)
                return r.Unwrap();
            else
                return InnerStore;
        }
    }
}