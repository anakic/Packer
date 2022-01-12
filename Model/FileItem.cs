using System.Text;

namespace Packer.Model
{
    class FileItem : FileSystemItem
    {
        protected string AbsolutePath { get; }

        public FileItem(string basePath, string relativePath)
            : base(relativePath)
        {
            AbsolutePath = System.IO.Path.Combine(basePath, relativePath);
        }

        //public override void Delete()
        //{
        //    File.Delete(AbsolutePath);
        //}

        public string ReadAsString(Encoding? encoding = null)
        {
            if (encoding != null)
                return File.ReadAllText(AbsolutePath, encoding);
            else
                return File.ReadAllText(AbsolutePath);
        }

        public byte[] ReadBytes()
        {
            return File.ReadAllBytes(AbsolutePath);
        }

        public void Save(string str, Encoding? encoding = null)
        {
            if (encoding != null)
                File.WriteAllText(AbsolutePath, str, encoding);
            else
                File.WriteAllText(AbsolutePath, str);
        }

        public void Save(byte[] bytes)
        {
            File.WriteAllBytes(AbsolutePath, bytes);
        }

        public void CopyRelativeTo(string otherBasePath)
        {
            File.Copy(AbsolutePath, System.IO.Path.Combine(otherBasePath, Path));
        }
    }
}
