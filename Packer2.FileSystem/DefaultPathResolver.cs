namespace Packer2.FileSystem
{
    /// <summary>
    /// Windows File System path navigator
    /// </summary>
    public class DefaultPathResolver : IPathResolver
    {
        static DefaultPathResolver _instance;
        public static DefaultPathResolver Instance { get => _instance ?? (_instance = new DefaultPathResolver()); }
        private DefaultPathResolver() { }

        public bool ArePathsEqual(string path1, string path2)
        {
            return NormalizePath(path1) == NormalizePath(path2);
        }

        private static string? NormalizePath(string path)
        {
            return path?
                .ToUpper()
                .TrimEnd('\\')
                .Replace("\\.\\", "\\");
        }

        public string CombinePath(string path, string child)
        {
            return Path.Combine(path, child);
        }

        public string GetName(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetParent(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public bool IsRootPath(string path)
        {
            return string.IsNullOrEmpty(path);
        }
    }
}