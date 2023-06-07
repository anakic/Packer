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

        interface IUntouchedFilesList
        {
            void MarkFileAsInUse(string path);
            void MarkFolderAsInUse(string path);

            public IEnumerable<string> GetUnusedFiles();
            public IEnumerable<string> GetUnusedFolders();
        }

        class UntouchedFilesList : IUntouchedFilesList
        {
            HashSet<string>? foldersForDeletion;
            HashSet<string>? filesForDeletion;

            public UntouchedFilesList(HashSet<string> filesForDeletion, HashSet<string> foldersForDeletion)
            {
                this.foldersForDeletion = foldersForDeletion;
                this.filesForDeletion = filesForDeletion;
            }

            public void MarkFileAsInUse(string path)
            {
                filesForDeletion.Remove(path);
            }

            public void MarkFolderAsInUse(string path)
            {
                foldersForDeletion!.Remove(path);
            }

            public IEnumerable<string> GetUnusedFiles() => filesForDeletion;
            public IEnumerable<string> GetUnusedFolders() => foldersForDeletion;
        }

        class SubUntouchedFilesList : IUntouchedFilesList
        {
            private readonly FolderModelStore<T>.IUntouchedFilesList inner;
            private readonly string subPath;
            private readonly IPathResolver resolver;

            public SubUntouchedFilesList(IUntouchedFilesList inner, string subPath, IPathResolver resolver)
            {
                this.inner = inner;
                this.subPath = subPath;
                this.resolver = resolver;
            }

            public void MarkFileAsInUse(string path)
            {
                inner.MarkFileAsInUse(resolver.CombinePath(subPath, path));
            }

            public void MarkFolderAsInUse(string path)
            {
                inner.MarkFolderAsInUse(resolver.CombinePath(subPath, path));
            }
            public IEnumerable<string> GetUnusedFiles() => throw new NotImplementedException("Not needed");
            public IEnumerable<string> GetUnusedFolders() => throw new NotImplementedException("Not needed");
        }


        class ProtectedFileSystemDecorator : IFileSystem
        {
            private readonly IFileSystem inner;
            private readonly HashSet<string> protectedFolders;
            private readonly HashSet<string> protectedFiles;
            IUntouchedFilesList? untouchedFilesList;

            public ProtectedFileSystemDecorator(IFileSystem inner, IEnumerable<string> protectedFolders, IEnumerable<string> protectedFiles)
            {
                this.protectedFolders = protectedFolders.ToHashSet(StringComparer.OrdinalIgnoreCase);
                this.protectedFiles = protectedFiles.ToHashSet(StringComparer.OrdinalIgnoreCase);
                this.inner = inner;
            }

            public void BeginUpdate()
            {
                var filesForDeletion = GetFilesRecursive("").ToHashSet(StringComparer.OrdinalIgnoreCase);
                var foldersForDeletion = GetFoldersRecursive("").ToHashSet(StringComparer.OrdinalIgnoreCase);
                untouchedFilesList = new UntouchedFilesList(filesForDeletion, foldersForDeletion);
            }
            public void EndUpdate()
            {
                if (untouchedFilesList == null)
                    throw new InvalidOperationException("EndUpdate called without BeginUpdate being executed.");

                var filesForDeletion = untouchedFilesList.GetUnusedFiles();
                var foldersForDeletion = untouchedFilesList.GetUnusedFolders();

                foreach (var file in filesForDeletion)
                    inner.DeleteFile(file);
                foreach (var folder in foldersForDeletion)
                    inner.DeleteFolder(folder);

                untouchedFilesList = null;
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
                => new ProtectedFileSystemDecorator(inner.Sub(childFolderPath), protectedFolders, protectedFiles) { untouchedFilesList = this.untouchedFilesList == null ? null : new SubUntouchedFilesList(untouchedFilesList, childFolderPath, PathResolver) };

            private void VerifyAccess(string path)
            {
                if (IsPathInProtectedAreas(path))
                    throw new AccessViolationException();
            }

            private void MarkFileAsInUse(string path)
            {
                untouchedFilesList?.MarkFileAsInUse(path);
                MarkFolderAsInUse(inner.PathResolver.GetParent(path));
            }

            private void MarkFolderAsInUse(string path)
            {
                untouchedFilesList?.MarkFolderAsInUse(path);
                foreach (var folder in inner.PathResolver.GetAncestors(path))
                    untouchedFilesList?.MarkFolderAsInUse(folder);
            }

            private bool IsPathInProtectedAreas(string path)
                => protectedFolders.Any(pf => inner.PathResolver.IsEqualToOrDescendantOf(path, pf)) 
                || protectedFiles.Any(pf => inner.PathResolver.ArePathsEqual(path, pf));
        }
    }
}
