using Newtonsoft.Json.Linq;
using Packer.Storage;
using System.Text;

namespace Packer.Model
{
    class JsonFileItem : FileSystemItem
    {
        public JObject JObj { get; set; }

        public Newtonsoft.Json.Formatting Formatting { get; set; }

        public JsonFileItem(string path, JObject jObj) 
            : base(path)
        {
            JObj = jObj;
        }

        internal override void SaveForMachine(IFilesStore store)
        {
            var str = JObj.ToString(Newtonsoft.Json.Formatting.None);
            var bytes = Encoding.Unicode.GetBytes(str);
            store.Write(Path, bytes);
        }

        internal override void SaveForHuman(IFilesStore store)
        {
            var str = JObj.ToString(Newtonsoft.Json.Formatting.Indented);
            store.Write(Path, str);
        }

        public static JsonFileItem Read(string path, IFilesStore store)
        {
            var bytes = store.ReadAsBytes(path);
            var jObj = ParseJsonBytes(bytes);
            return new JsonFileItem(path, jObj);
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
