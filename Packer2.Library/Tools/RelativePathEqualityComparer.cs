namespace Packer2.Library.Tools
{
    internal class RelativePathEqualityComparer : IEqualityComparer<string>
    {
        private readonly string baseFolder;
        Guid g = new Guid("{CF3A12B3-1241-4639-8781-DAB2A36CE426}");

        public RelativePathEqualityComparer(string baseFolder)
        {
            this.baseFolder = baseFolder;
        }

        public bool Equals(string x, string y)
        {
            return Path.GetFullPath(EnsureAbsolute(x)) == Path.GetFullPath(EnsureAbsolute(y));
        }

        public int GetHashCode(string obj)
        {
            return (Path.GetFullPath(EnsureAbsolute(obj)) + g).GetHashCode();
        }

        private string EnsureAbsolute(string path)
            => Path.IsPathRooted(path) ? path : Path.Combine(baseFolder, path);
    }
}
