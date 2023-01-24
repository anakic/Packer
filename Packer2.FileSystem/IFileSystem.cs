namespace Packer2.FileSystem
{
    /// <summary>
    /// Represents a hierarchical file system. Includes inspection and navigation 
    /// functionality and methods for writing/reading data to/from files.
    /// </summary>
    public interface IFileSystem
    {
        string Kind { get; }

        string ReadAsString(string filePath);
        byte[] ReadAsBytes(string filePath);
        void Save(string filePath, string content);
        void Save(string filePath, byte[] content);
        DateTime GetLastChangedTime(string filePath);

        // inspecting files
        IEnumerable<string> GetFiles(string folderPath);
        IEnumerable<string> GetFilesRecursive(string folderPath);
        bool FileExists(string path);
        void DeleteFile(string path);
        void MoveFile(string path, string newPath);

        // inspecting folders
        IEnumerable<string> GetFolders(string path);
        IEnumerable<string> GetFoldersRecursive(string folderPath);
        bool FolderExists(string path);
        void DeleteFolder(string path);
        void CreateFolder(string path);
        void MoveFolder(string originalPath, string newPath);

        // path navigation mechanism
        IPathResolver PathResolver { get; }

        IFileSystem Sub(string childFolderPath);
    }
}