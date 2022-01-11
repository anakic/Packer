using Newtonsoft.Json.Linq;
using System.Text;

namespace Packer.Tools
{
    internal static class JsonFileHelpers
    {
        public static IEnumerable<string> GetAllJsonFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*")
                .Where(f => string.IsNullOrEmpty(Path.GetExtension(f)) && !string.Equals(Path.GetFileName(f), "Version", StringComparison.OrdinalIgnoreCase))
                .Union(Directory.GetFiles(Path.Combine(folderPath, "Report"), "*", SearchOption.AllDirectories));
        }

        public static JObject ParseJsonStr(byte[] bytes)
        {
            var encodingsToTry = new[] { Encoding.Unicode, Encoding.UTF8 };
            foreach (var enc in encodingsToTry)
            {
                try
                {
                    return JObject.Parse(enc.GetString(bytes));
                }
                catch
                {
                    // ignored
                }
            }
            throw new Exception("Unknown encoding or error in json string");
        }
    }
}
