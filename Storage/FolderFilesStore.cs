using System.IO.Compression;

namespace Packer.Storage
{
    internal class FolderFilesStore : IFilesStore
    {
        private readonly string folderPath;

        public FolderFilesStore(string folderPath)
        {
            this.folderPath = folderPath;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        // clears all files except the .git folder
        public void UpdateContentsFromZip(string zipFile)
        {
            // clear everything except the .git folder
            foreach (var d in Directory.GetDirectories(folderPath))
            {
                if (Path.GetFileName(d) == ".git")
                    continue;
                Directory.Delete(d, true);
            }
            foreach (var f in Directory.GetFiles(folderPath))
            {
                File.Delete(f);
            }

            ZipFile.ExtractToDirectory(zipFile, folderPath);
        }

        public bool FileExists(string path)
        {
            return File.Exists(ToAbsolute(path));
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return Directory.GetFiles(ToAbsolute(path));
        }

        public IEnumerable<string> GetFolders(string path)
        {
            return Directory.GetDirectories(ToAbsolute(path))
                .Where(d => !d.EndsWith(".git"));
        }

        public byte[] ReadAsBytes(string path)
        {
            return File.ReadAllBytes(ToAbsolute(path));
        }

        public string ReadAsText(string path)
        {
            return File.ReadAllText(ToAbsolute(path));
        }

        public void Write(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public void Write(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }

        private string ToAbsolute(string path)
        {
            return Path.Combine(folderPath, path);
        }
    }
}
