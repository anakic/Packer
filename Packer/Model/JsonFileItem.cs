using Newtonsoft.Json.Linq;
using Packer.Storage;
using System.Text;

namespace Packer.Model
{
    /// <summary>
    /// Represents a json file in the repo. Exposes a JObject property 
    /// that can be modified. 
    /// </summary>
    public class JsonFileItem : FileSystemItem
    {
        private readonly bool noExtensionInPbit;

        public JObject JObj { get; set; }

        public JsonFileItem(string path, JObject jObj, bool noExtensionInPbit = true) 
            : base(path)
        {
            JObj = jObj;
            this.noExtensionInPbit = noExtensionInPbit;
        }

        internal override void SaveForMachine(IFilesStore store)
        {
            var str = JObj.ToString(Newtonsoft.Json.Formatting.None);
            var bytes = Encoding.Unicode.GetBytes(str);
            var path = Path;
            if (noExtensionInPbit)
            {
                if (System.IO.Path.GetExtension(Path).ToLower() != ".json")
                    throw new FormatException("Was expecting a file with .json extension");
                path = path.Substring(0, path.Length - ".json".Length);
            }
            store.Write(path, bytes);
        }

        internal override void SaveForHuman(IFilesStore store)
        {
            var str = JObj.ToString(Newtonsoft.Json.Formatting.Indented);

            var path = Path;
            if (!string.Equals(System.IO.Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase))
                path += ".json";

            store.Write(path, str);
        }

        public static JsonFileItem Read(string path, IFilesStore store)
        {
            var bytes = store.ReadAsBytes(path);
            var jObj = ParseJsonBytes(bytes);
            return new JsonFileItem(path, jObj, !System.IO.Path.HasExtension("json"));
        }

        static JObject ParseJsonBytes(byte[] bytes)
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
