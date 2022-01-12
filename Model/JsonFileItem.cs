using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.Model
{
    class JsonFileItem : FileItem
    {
        Lazy<JObject> lazyObj;
        public JsonFileItem(string basePath, string relativePath) : base(basePath, relativePath)
        {
            lazyObj = new Lazy<JObject>(() => ParseJsonStr(ReadBytes()));
        }

        public void Modify(Action<JObject> action, Newtonsoft.Json.Formatting formatting)
        {
            action(lazyObj.Value);
            var str = lazyObj.Value.ToString(formatting);
            var bytes = Encoding.Unicode.GetBytes(str);
            Save(bytes);
        }

        static JObject ParseJsonStr(byte[] bytes)
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
