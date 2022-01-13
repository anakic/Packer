namespace Packer
{
    internal class Utils
    {
        public static string GetRelativePath(string baseFolder, string f)
        {
            return f.Substring(baseFolder.TrimEnd('\\').Length + 1);
        }
    }
}
