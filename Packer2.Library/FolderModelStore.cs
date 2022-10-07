using Packer2.Library.Tools;

namespace Packer2.Library
{
    public abstract class FolderModelStore<T> : IModelStore<T>
    {
        private readonly string folder;

        public FolderModelStore(string folder)
        {
            this.folder = folder;
        }

        protected virtual bool IsProtectedFolder(string relativePath)
            => relativePath == ".git";

        protected virtual bool IsProtectedFile(string relativePath) => false;

        // todo: add logging
        private void ClearFolder()
        {
            if (Directory.Exists(folder))
            {
                foreach (var childDir in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    // do not remove the .git folder
                    if (!IsProtectedFolder(PathTools.GetRelativePath(childDir, folder)))
                        Directory.Delete(childDir, true);
                }

                foreach (var file in Directory.GetFiles(folder))
                {
                    if(!IsProtectedFile(PathTools.GetRelativePath(file, folder)))
                        File.Delete(file);
                }
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
