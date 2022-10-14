namespace Packer2.FileSystem
{
    public static class IDataStoreExtensions
    {
        public static bool TryFindFile(this IFileSystem store, string fileName, out string path, string containingFolder = "")
        {
            path = store
                .GetFilesRecursive(containingFolder)
                .SingleOrDefault(f => store.PathResolver.ArePathsEqual(fileName, store.PathResolver.GetName(f)));
            return path != null;
        }

        public static string FindFile(this IFileSystem store, string fileName, string containingFolder = "")
        {
            if (TryFindFile(store, fileName, out var res, containingFolder))
                return res;
            else
                throw new FileNotFoundException();
        }

        public static IEnumerable<string> FindFiles(this IFileSystem store, string fileName, string containingFolder = "")
        {
            return store.GetFilesRecursive(containingFolder)
                .Where(f => store.PathResolver.ArePathsEqual(fileName, store.PathResolver.GetName(f)));
        }

        public static string Dump(this IFileSystem store)
        {
            return store.GetFilesRecursive("").Aggregate("", (a, f) => a + f + Environment.NewLine);
        }
    }
}