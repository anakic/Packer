namespace Packer2.Library.Tools
{
    internal class TempFolder : IDisposable
    {
        const string TempSubFolderRoot = "Packer2";

        static string rootDir;
        static TempFolder()
        {
            rootDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), TempSubFolderRoot);
            if (Directory.Exists(rootDir))
            {
                try { Directory.Delete(rootDir, true); }
                catch { }
            }
        }

        public string Path { get; }

        public TempFolder()
            : this(Guid.NewGuid().ToString())
        { }

        public TempFolder(string purposeId)
        {
            Path = System.IO.Path.Combine(rootDir, purposeId);
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch { }
        }
    }
}
