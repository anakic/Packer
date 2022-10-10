using Newtonsoft.Json.Linq;

namespace Packer2.Library.Tools
{
    class FileTools
    {
        public static void WriteToFile(string path, JToken obj)
        {
            WriteToFile(path, obj.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public static void WriteToFile(string path, string text)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, text);
        }
    }
}
