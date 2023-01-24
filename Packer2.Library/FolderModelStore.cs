using Packer2.FileSystem;
using Packer2.Library.DataModel.Customizations;

namespace Packer2.Library
{
    public abstract class FolderModelStore<T> : IModelStore<T>
    {
        private readonly ProtectedFileSystemDecorator fileSystem;

        IFileSystem originalFileSystem;

        public FolderModelStore(IFileSystem fileSystem)
        {
            originalFileSystem = fileSystem;
            this.fileSystem = new ProtectedFileSystemDecorator(
                originalFileSystem, 
                new[] { ".git", ".config", CustFileSystem.CustomizationsFolder }, 
                new[] { CustFileSystem.IgnoreFile });
        }

        public void Save(T model)
        {
            // Begin/End update are used to clear old files
            // In "Begin" a list of files and folders is created
            // In "End" all files/folders that haven't been written to are cleared

            fileSystem.BeginUpdate();
            DoSave(model, fileSystem);
            fileSystem.EndUpdate();
        }

        protected string ReadIgnoreFile()
        {
            return (originalFileSystem.FileExists(CustFileSystem.IgnoreFile))
                ? originalFileSystem.ReadAsString(CustFileSystem.IgnoreFile)
                : string.Empty;
        }

        protected abstract void DoSave(T model, IFileSystem fileSystem);

        public T Read()
            => DoRead(fileSystem);

        protected abstract T DoRead(IFileSystem fileSystem);


        class ProtectedFileSystemDecorator : IFileSystem
        {
            private readonly IFileSystem inner;
            private readonly HashSet<string> protectedFolders;
            private readonly HashSet<string> protectedFiles;
            HashSet<string>? foldersForDeletion;
            HashSet<string>? filesForDeletion;

            public ProtectedFileSystemDecorator(IFileSystem inner, IEnumerable<string> protectedFolders, IEnumerable<string> protectedFiles)
            {
                this.protectedFolders = protectedFolders.ToHashSet(StringComparer.OrdinalIgnoreCase);
                this.protectedFiles = protectedFiles.ToHashSet(StringComparer.OrdinalIgnoreCase);
                this.inner = inner;
            }

            public void BeginUpdate()
            {
                filesForDeletion = GetFilesRecursive("").ToHashSet(StringComparer.OrdinalIgnoreCase);
                foldersForDeletion = GetFoldersRecursive("").ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            public void EndUpdate()
            {
                if (filesForDeletion == null || foldersForDeletion == null)
                    throw new InvalidOperationException("EndUpdate called without BeginUpdate being executed.");

                foreach (var file in filesForDeletion)
                    inner.DeleteFile(file);
                foreach (var folder in foldersForDeletion)
                    inner.DeleteFolder(folder);

                filesForDeletion = null;
                foldersForDeletion = null;
            }

            public string Kind => inner.Kind;

            public IPathResolver PathResolver => inner.PathResolver;

            public void CreateFolder(string path)
            {
                VerifyAccess(path);

                MarkFolderAsInUse(path);
                inner.CreateFolder(path);
            }

            public void DeleteFile(string path)
            {
                VerifyAccess(path);
                inner.DeleteFile(path);
            }

            public void DeleteFolder(string path)
            {
                VerifyAccess(path);
                inner.DeleteFolder(path);
            }

            public bool FileExists(string path)
            {
                VerifyAccess(path);
                return inner.FileExists(path);
            }

            public bool FolderExists(string path)
            {
                VerifyAccess(path);
                return inner.FolderExists(path);
            }

            public IEnumerable<string> GetFiles(string folderPath)
            {
                VerifyAccess(folderPath);
                return inner.GetFiles(folderPath);
            }

            public IEnumerable<string> GetFilesRecursive(string folderPath)
                => inner.GetFilesRecursive(folderPath)
                    // do not show files in protected areas
                    .Where(f => !IsPathInProtectedAreas(f));

            public IEnumerable<string> GetFolders(string path)
            {
                VerifyAccess(path);
                return inner.GetFolders(path)
                    // do not show protected folders
                    .Where(f => !IsPathInProtectedAreas(f));
            }


            public IEnumerable<string> GetFoldersRecursive(string folderPath)
            {
                VerifyAccess(folderPath);
                return inner.GetFoldersRecursive(folderPath)
                    // do not show protected folders
                    .Where(f => !IsPathInProtectedAreas(f));
            }


            public DateTime GetLastChangedTime(string filePath) => inner.GetLastChangedTime(filePath);

            public void MoveFile(string path, string newPath)
            {
                VerifyAccess(path);
                VerifyAccess(newPath);

                inner.MoveFile(path, newPath);
                MarkFileAsInUse(newPath);
            }

            public void MoveFolder(string originalPath, string newPath)
            {
                VerifyAccess(originalPath);
                VerifyAccess(newPath);

                inner.MoveFolder(originalPath, newPath);
                MarkFolderAsInUse(newPath);

                foreach (var f in GetFilesRecursive(newPath))
                    MarkFileAsInUse(f);
            }

            public byte[] ReadAsBytes(string filePath)
            {
                VerifyAccess(filePath);
                return inner.ReadAsBytes(filePath);
            }

            public string ReadAsString(string filePath)
            {
                VerifyAccess(filePath);
                return inner.ReadAsString(filePath);
            }

            public void Save(string filePath, string content)
            {
                VerifyAccess(filePath);
                inner.Save(filePath, content);
                MarkFileAsInUse(filePath);
            }

            public void Save(string filePath, byte[] content)
            {
                VerifyAccess(filePath);
                inner.Save(filePath, content);
                MarkFileAsInUse(filePath);
            }

            public IFileSystem Sub(string childFolderPath)
                => new ProtectedFileSystemDecorator(inner.Sub(childFolderPath), protectedFolders, protectedFiles);

            private void VerifyAccess(string path)
            {
                if (IsPathInProtectedAreas(path))
                    throw new AccessViolationException();
            }

            private void MarkFileAsInUse(string path)
            {
                filesForDeletion?.Remove(path);
                MarkFolderAsInUse(inner.PathResolver.GetParent(path));
            }

            private void MarkFolderAsInUse(string path)
            {
                foldersForDeletion?.Remove(path);
            }

            private bool IsPathInProtectedAreas(string path)
                => protectedFolders.Any(pf => inner.PathResolver.IsEqualToOrDescendantOf(path, pf)) 
                || protectedFiles.Any(pf => inner.PathResolver.ArePathsEqual(path, pf));
        }
    }
}
