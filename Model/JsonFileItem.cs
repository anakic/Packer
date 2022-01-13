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

        internal override byte[] GetBytesToSave()
        {
            var str = JObj.ToString(Formatting);
            return Encoding.Unicode.GetBytes(str);
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
