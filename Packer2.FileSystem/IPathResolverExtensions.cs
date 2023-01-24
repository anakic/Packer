namespace Packer2.FileSystem
{
    /// <summary>
    /// Useful extension methods for clients of IPathResolver. The methods here
    /// combine the methods from the interface for useful scenarios. They also
    /// deal with file extensions that the store itself does not care about.
    /// </summary>
    public static class IPathResolverExtensions
    {
        // File extensions are not anything meaningful to the store. They mean something to the user, but not to the store. 
        // Hence, they're not part of the IPathResolver interface, but are supported via extension methods.

        public static string StripExtension(this IPathResolver resolver, string name)
        {
            return Path.GetFileNameWithoutExtension(name);
        }

        public static string ChangeExtension(this IPathResolver resolver, string path, string extension)
        {
            return Path.ChangeExtension(path, extension);
        }

        public static string AppendExtension(this IPathResolver resolver, string name, string extension)
        {
            return string.IsNullOrEmpty(extension) ? name : $"{name}.{extension}";
        }

        public static bool HasExtension(this IPathResolver resolver, string path, string extension)
        {
            return resolver.ArePathsEqual(resolver.GetExtension(path), extension);
        }

        public static bool HasName(this IPathResolver resolver, string path, string name)
        {
            return resolver.ArePathsEqual(resolver.GetName(path), name);
        }

        public static string GetExtension(this IPathResolver resolver, string path)
        {
            var indexOfDot = path.LastIndexOf('.');
            if (indexOfDot == -1)
                return "";
            else
                return path.Substring(indexOfDot + 1);
        }

        public static string GetFileNameWithoutExtension(this IPathResolver resolver, string path)
        {
            return resolver.StripExtension(resolver.GetName(path));
        }

        public static string CombinePaths(this IPathResolver resolver, string basePath, params string[] segments)
        {
            string current = basePath;
            foreach (var seg in segments)
                current = resolver.CombinePath(current, seg);
            return current;
        }

        public static string ChangeFolderName(this IPathResolver resolver, string path, string newName)
        {
            return resolver.CombinePath(resolver.GetParent(path), newName);
        }

        public static string ChangeName(this IPathResolver resolver, string path, string newName)
        {
            string extension = resolver.GetExtension(path);
            return resolver.CombinePath(resolver.GetParent(path), resolver.AppendExtension(newName, extension));
        }

        public static string ChangeName(this IPathResolver resolver, string path, Func<string, string> nameTransformFunc)
        {
            string extension = resolver.GetExtension(path);
            string currentName = resolver.GetFileNameWithoutExtension(path);
            return resolver.CombinePath(resolver.GetParent(path), resolver.AppendExtension(nameTransformFunc(currentName), extension));
        }

        public static string ConstructPath(this IPathResolver resolver, string folderPath, string name, string extension)
        {
            return resolver.CombinePath(folderPath, resolver.AppendExtension(name, extension));
        }

        public static string ChangeFileParent(this IPathResolver resolver, string filePath, string newParentPath)
        {
            return resolver.CombinePath(newParentPath, resolver.GetName(filePath));
        }

        public static bool IsDescendantOf(this IPathResolver resolver, string path, string potentialAncestor)
        {
            string remainder = path;

            while (!resolver.IsRootPath(remainder))
            {
                remainder = resolver.GetParent(remainder);
                if (resolver.ArePathsEqual(remainder, potentialAncestor))
                    return true;
            }

            return false;
        }

        public static bool IsEqualToOrDescendantOf(this IPathResolver resolver, string path, string potentialAncestor)
            => resolver.ArePathsEqual(path, potentialAncestor) || resolver.IsDescendantOf(path, potentialAncestor);

        public static IEnumerable<string> GetAncestors(this IPathResolver resolver, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                yield break;

            string current = filePath;
            while (!string.IsNullOrEmpty(current = resolver.GetParent(current)))
                yield return current;
        }

        public static IEnumerable<string> GetPathSegments(this IPathResolver resolver, string path) => (path ?? "").Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        public static bool IsChildOf(this IPathResolver resolver, string path, string potentialParent)
        {
            return !resolver.IsRootPath(path) && resolver.ArePathsEqual(resolver.GetParent(path), potentialParent);
        }

        public static string GetRelativePath(this IPathResolver resolver, string path, string basePath)
        {
            string result = string.Empty;
            string remainder = path;

            while (!resolver.ArePathsEqual(remainder, basePath))
            {
                if (resolver.IsRootPath(remainder))
                    throw new ArgumentException($"Path '{path}' is not a child of path '{basePath}'");

                result = resolver.CombinePath(resolver.GetName(remainder), result);
                remainder = resolver.GetParent(remainder);
            }

            return result;
        }
    }
}