using System.Text;

namespace Packer2.FileSystem
{
    public class MemoryFileSystem : IFileSystem
    {
        public const string StoreKind = "Memory";
        public string Kind => StoreKind;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        HashSet<string> folders = new HashSet<string>() { "" };
        Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();
        Dictionary<string, DateTime> dataTimestamps = new Dictionary<string, DateTime>();

        public IPathResolver PathResolver => DefaultPathResolver.Instance;

        public void CreateFolder(string folderPath)
        {
            folders.Add(folderPath);
        }

        public void DeleteFile(string name)
        {
            data.Remove(name);
        }

        public void DeleteFolder(string path)
        {
            folders.Remove(path);
            data.Where(kvp => PathResolver.IsDescendantOf(kvp.Key, path))
                .Select(kvp => kvp.Key)
                .ToList()
                .ForEach(DeleteFile);
        }

        public bool FileExists(string filePath)
        {
            return data.ContainsKey(filePath);
        }

        public bool FolderExists(string path)
        {
            return folders.Contains(path);
        }

        public IEnumerable<string> GetFiles(string folderPath)
        {
            return data.Where(kvp => PathResolver.IsChildOf(kvp.Key, folderPath)).Select(kvp => kvp.Key);
        }

        public IEnumerable<string> GetFilesRecursive(string folderPath)
        {
            return data.Where(kvp => PathResolver.IsDescendantOf(kvp.Key, folderPath)).Select(kvp => kvp.Key);
        }

        public IEnumerable<string> GetFolders(string folderPath)
        {
            return folders.Where(f => PathResolver.IsChildOf(f, folderPath));
        }

        public void MoveFile(string path, string newPath)
        {
            data[newPath] = data[path];
            data.Remove(path);
        }

        public void MoveFolder(string originalPath, string newPath)
        {
            foreach (var file in data.Keys.ToArray())
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
            return data[filePath];
        }

        public string ReadAsString(string filePath)
        {
            return Encoding.GetString(data[filePath]);
        }

        public void Save(string filePath, string content)
        {
            folders.Add(PathResolver.GetParent(filePath));
            data[filePath] = Encoding.GetBytes(content);
            dataTimestamps[filePath] = DateTime.Now;
        }

        public void Save(string filePath, byte[] content)
        {
            folders.Add(PathResolver.GetParent(filePath));
            data[filePath] = content;
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