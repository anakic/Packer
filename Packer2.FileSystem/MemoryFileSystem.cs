using System.Text;

namespace Packer2.FileSystem
{
    public class MemoryFileSystem : IFileSystem
    {
        public const string StoreKind = "Memory";
        public string Kind => StoreKind;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        HashSet<string> folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "" };
        Dictionary<string, byte[]> files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, DateTime> dataTimestamps = new Dictionary<string, DateTime>();

        public IPathResolver PathResolver => DefaultPathResolver.Instance;

        public void CreateFolder(string folderPath)
        {
            folders.Add(folderPath);
        }

        public void DeleteFile(string name)
        {
            files.Remove(name);
        }

        public void DeleteFolder(string path)
        {
            folders.Remove(path);
            files.Where(kvp => PathResolver.IsDescendantOf(kvp.Key, path))
                .Select(kvp => kvp.Key)
                .ToList()
                .ForEach(DeleteFile);
        }

        public bool FileExists(string filePath)
        {
            return files.ContainsKey(filePath);
        }

        public bool FolderExists(string path)
        {
            return folders.Contains(path);
        }

        public IEnumerable<string> GetFiles(string folderPath)
        {
            return files.Where(kvp => PathResolver.IsChildOf(kvp.Key, folderPath)).Select(kvp => kvp.Key);
        }

        public IEnumerable<string> GetFilesRecursive(string folderPath)
        {
            return files.Where(kvp => PathResolver.IsDescendantOf(kvp.Key, folderPath)).Select(kvp => kvp.Key);
        }

        public IEnumerable<string> GetFoldersRecursive(string folderPath)
        {
            return folders.Where(f => PathResolver.IsDescendantOf(f, folderPath));
        }

        public IEnumerable<string> GetFolders(string folderPath)
        {
            return folders.Where(f => PathResolver.IsChildOf(f, folderPath));
        }

        public void MoveFile(string path, string newPath)
        {
            files[newPath] = files[path];
            files.Remove(path);
        }

        public void MoveFolder(string originalPath, string newPath)
        {
            foreach (var file in files.Keys.ToArray())
            {
                if (PathResolver.IsDescendantOf(file, originalPath))
                {
                    string relativePath = PathResolver.GetRelativePath(file, originalPath);
                    string newFilePath = PathResolver.CombinePath(newPath, relativePath);
                    MoveFile(file, newFilePath);
                }
            }
            folders.Remove(originalPath);
            folders.Add(newPath);
        }

        public byte[] ReadAsBytes(string filePath)
        {
            return files[filePath];
        }

        public string ReadAsString(string filePath)
        {
            return Encoding.GetString(files[filePath]);
        }

        public void Save(string filePath, string content)
        {
            folders.Add(PathResolver.GetParent(filePath));
            files[filePath] = Encoding.GetBytes(content);
            dataTimestamps[filePath] = DateTime.Now;
        }

        public void Save(string filePath, byte[] content)
        {
            folders.Add(PathResolver.GetParent(filePath));
            files[filePath] = content;
            dataTimestamps[filePath] = DateTime.Now;
        }

        public void SetLastChangedTime(string filePath, DateTime newTimeStamp)
        {
            dataTimestamps[filePath] = newTimeStamp;
        }

        public DateTime GetLastChangedTime(string filePath)
        {
            return dataTimestamps[filePath];
        }

        public IFileSystem Sub(string childFolderPath)
            => new RelativeFileSystem(this, childFolderPath);
    }
}