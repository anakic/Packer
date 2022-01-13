using System.IO.Compression;
using System.Text;

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
            string absPath = ToAbsolute(path);
            if(Directory.Exists(absPath))
                return Directory.GetFiles(absPath).Select(ToRelative);
            else
                return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetFolders(string path)
        {
            return Directory.GetDirectories(ToAbsolute(path))
                .Where(d => !d.EndsWith(".git"))
                .Select(ToRelative);
        }

        public byte[] ReadAsBytes(string path)
        {
            return File.ReadAllBytes(ToAbsolute(path));
        }

        public string ReadAsText(string path, Encoding? encoding = null)
        {
            if(encoding == null)
                return File.ReadAllText(ToAbsolute(path));
            else
                return File.ReadAllText(ToAbsolute(path), encoding);
        }

        public void Write(string path, string text)
        {
            File.WriteAllText(ToAbsolute(path), text);
        }

        public void Write(string path, byte[] bytes)
        {
            var absPath = ToAbsolute(path);
            var dirName = Path.GetDirectoryName(absPath)!;
            if(!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
            File.WriteAllBytes(absPath, bytes);
        }

        private string ToRelative(string path)
            => path[(folderPath.TrimEnd('\\').Length + 1)..];

        private string ToAbsolute(string path)
        {
            return Path.Combine(folderPath, path);
        }
    }
}
