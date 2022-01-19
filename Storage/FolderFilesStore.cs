using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Packer.Storage
{
    /// <summary>
    /// Represents the file system that contains the repository files. This will be a folder on
    /// the local file system. This class must not expose the .git subdirectory and must also
    /// ensure that all paths are relative to the base folder.
    /// </summary>
    internal class FolderFilesStore : IFilesStore
    {
        private readonly string folderPath;

        public string EscapeName(string name)
        {
            // todo: this could cause problems if we have two names with different special 
            // characters in the same location since they would result in the same escaped name.
            // we might want to use a different mapping (1:1) instead of (N:1), for exmaple
            // using rare unicode chars or some encoding scheme. 
            return Regex.Replace(name, @"[<>/\:""\|?*]", "_");
        }

        public FolderFilesStore(string folderPath)
        {
            this.folderPath = folderPath;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        // clears all files except the .git folder
        public void UpdateContentsFromZip(string zipFile)
        {
            ClearContents();
            ZipFile.ExtractToDirectory(zipFile, folderPath);
        }

        public void ClearContents()
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
        }

        public bool FileExists(string path)
        {
            return File.Exists(ToAbsolute(path));
        }

        public IEnumerable<string> GetFiles(string path)
        {
            string absPath = ToAbsolute(path);
            if (Directory.Exists(absPath))
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
            if (encoding == null)
                return File.ReadAllText(ToAbsolute(path));
            else
                return File.ReadAllText(ToAbsolute(path), encoding);
        }

        public void Write(string path, string text, Encoding? encoding = null)
        {
            var absPath = ToAbsolute(path);
            EnsureParentFolderExists(absPath);
            if (encoding == null)
                File.WriteAllText(absPath, text);
            else
                File.WriteAllText(absPath, text, encoding);
        }

        public void Write(string path, byte[] bytes)
        {
            var absPath = ToAbsolute(path);
            EnsureParentFolderExists(absPath);
            File.WriteAllBytes(absPath, bytes);
        }

        private static void EnsureParentFolderExists(string absPath)
        {
            var dirName = Path.GetDirectoryName(absPath)!;
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
        }

        private string ToRelative(string path)
            => path[(folderPath.TrimEnd('\\').Length + 1)..];

        private string ToAbsolute(string path)
        {
            return Path.Combine(folderPath, EscapeName(path));
        }
    }
}
