namespace Packer2.FileSystem
{
    public interface IPathResolver
    {
        string GetParent(string path);
        string GetName(string path);

        string CombinePath(string path, string child);

        bool ArePathsEqual(string path1, string path2);
        bool IsRootPath(string path);
    }
}