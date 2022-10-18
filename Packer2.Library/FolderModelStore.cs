using Packer2.FileSystem;

namespace Packer2.Library
{
    public abstract class FolderModelStore<T> : IModelStore<T>
    {
        private readonly IFileSystem fileSystem;

        public FolderModelStore(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        protected virtual bool IsProtectedFolder(string relativePath)
            => relativePath == ".git" || relativePath == ".config";

        protected virtual bool IsProtectedFile(string relativePath) => false;

        // todo: add logging
        private void ClearFolder()
        {
            foreach (var childDir in fileSystem.GetFolders(""))
            {
                // do not remove protected folders (i.e. the .git and .config folders)
                if (!IsProtectedFolder(childDir))
                    fileSystem.DeleteFolder(childDir);
            }

            foreach (var file in fileSystem.GetFiles(""))
            {
                if(!IsProtectedFile(file))
                    fileSystem.DeleteFile(file);
            }
        }

        public void Save(T model)
        {
            ClearFolder();
            DoSave(model);
        }

        protected abstract void DoSave(T model);

        public abstract T Read();
    }
}
