using System.Text;

namespace Packer.Storage
{
    /// <summary>
    /// Represents the file system that contains the repository files.
    /// </summary>
    public interface IFilesStore
    {
        public string EscapeName(string name);

        public IEnumerable<string> GetFiles(string path);
        public IEnumerable<string> GetFolders(string path);
        public string ReadAsText(string path, Encoding? encoding = null);
        public byte[] ReadAsBytes(string path);
        public void Write(string path, string text, Encoding? encoding = null);
        public void Write(string path, byte[] bytes);
        bool FileExists(string path);
    }
}
