using Newtonsoft.Json.Linq;

namespace Packer2.Library.Tools
{
    class FileTools
    {
        public static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static void WriteToFile(string path, JToken obj)
        {
            WriteToFile(path, obj.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public static void WriteToFile(string path, string text)
        {
            var dir = Path.GetDirectoryName(path)!;
            EnsureDirectoryExists(dir);
            File.WriteAllText(path, text);
        }

        public static void WriteToFile(string path, byte[] bytes)
        {
            var dir = Path.GetDirectoryName(path)!;
            EnsureDirectoryExists(dir);
            File.WriteAllBytes(path, bytes);
        }
    }
}
