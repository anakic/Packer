namespace Packer2.Library.Tools
{
    internal class PathTools
    {
        public static string GetRelativePath(string path, string baseFolder)
        {
            Uri pathUri = new Uri(path);
            // Folders must end in a slash
            if (!baseFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                baseFolder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(baseFolder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
