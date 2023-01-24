using Packer2.FileSystem;
using System.Text;

namespace Packer2.Library.DataModel.Customizations
{
    public class CustFileSystem : IFileSystem
    {
        public const string CustomizationsFolder = ".cust";
        public const string IgnoreFile = "packer.ignore";

        private readonly IFileSystem rootFS;
        private readonly IFileSystem custFS;

        public string Kind => rootFS.Kind;

        public IPathResolver PathResolver => rootFS.PathResolver;

        public CustFileSystem(IFileSystem rootFS, string customizationName)
        {
            this.rootFS = rootFS;
            this.custFS = rootFS.Sub(rootFS.PathResolver.CombinePath(CustomizationsFolder, customizationName));
        }

        public void CreateFolder(string path)
        {
            rootFS.CreateFolder(path);
        }

        public void DeleteFile(string path)
        {
            rootFS?.DeleteFile(path);
            custFS?.DeleteFile(path);
        }

        public void DeleteFolder(string path)
        {
            rootFS.DeleteFolder(path);
            custFS.DeleteFolder(path);
        }

        public bool FileExists(string path)
        {
            return custFS.FileExists(path) || rootFS.FileExists(path);
        }

        public bool FolderExists(string path)
        {
            return custFS.FolderExists(path) || rootFS.FolderExists(path);
        }

        public IEnumerable<string> GetFiles(string folderPath)
        {
            return rootFS.GetFiles(folderPath).Union(custFS.GetFiles(folderPath));
        }

        public IEnumerable<string> GetFilesRecursive(string folderPath)
        {
            return rootFS.GetFilesRecursive(folderPath).Union(custFS.GetFilesRecursive(folderPath));
        }

        public IEnumerable<string> GetFolders(string path)
        {
            return rootFS.GetFolders(path).Union(custFS.GetFolders(path));
        }

        public IEnumerable<string> GetFoldersRecursive(string folderPath)
        {
            return rootFS.GetFoldersRecursive(folderPath).Union(custFS.GetFoldersRecursive(folderPath));
        }

        public DateTime GetLastChangedTime(string filePath)
        {
            if (custFS.FileExists(filePath))
                return custFS.GetLastChangedTime(filePath);
            else
                return rootFS.GetLastChangedTime(filePath);
        }

        public void MoveFile(string path, string newPath)
        {
            if (custFS.FileExists(path))
                custFS.MoveFile(path, newPath);
            else
                rootFS.MoveFile(path, newPath);
        }

        public void MoveFolder(string originalPath, string newPath)
        {
            if (custFS.FolderExists(originalPath))
                custFS.MoveFolder(originalPath, newPath);
            else
                rootFS.MoveFolder(originalPath, newPath);
        }

        public byte[] ReadAsBytes(string filePath)
        {
            if (rootFS.PathResolver.ArePathsEqual(filePath, IgnoreFile))
                throw new NotImplementedException("Not implemented at the momeent. Use ReadAsString for packer.ignore file.");

            if (custFS.FileExists(filePath))
                return custFS.ReadAsBytes(filePath);
            else
                return rootFS.ReadAsBytes(filePath);
        }

        public string ReadAsString(string filePath)
        {
            if (rootFS.PathResolver.ArePathsEqual(filePath, IgnoreFile))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# Global rules");
                if (rootFS.FileExists(filePath))
                    sb.AppendLine(rootFS.ReadAsString(filePath));
                sb.AppendLine("# Customization override rules");
                if(custFS.FileExists(filePath))
                    sb.AppendLine(custFS.ReadAsString(filePath));
                return sb.ToString();
            }

            if (custFS.FileExists(filePath))
                return custFS.ReadAsString(filePath);
            else
                return rootFS.ReadAsString(filePath);
        }

        public void Save(string filePath, string content)
        {
            if (custFS.FileExists(filePath))
                custFS.Save(filePath, content);
            else
                rootFS.Save(filePath, content);
        }

        public void Save(string filePath, byte[] content)
        {
            if (custFS.FileExists(filePath))
                custFS.Save(filePath, content);
            else
                rootFS.Save(filePath, content);
        }

        public IFileSystem Sub(string childFolderPath)
        {
            throw new NotImplementedException();
        }
    }
}
