using Newtonsoft.Json.Linq;
using System.Text;

namespace Packer.TMP
{
    class BimModelStore : IModelStore<DataModel>
    {
        private readonly string file;

        public BimModelStore(string file)
        {
            this.file = file;
        }

        public DataModel Read()
        {
            var jObj = JObject.Parse(File.ReadAllText(file, Encoding.Unicode));
            return new DataModel(jObj);
        }

        public void Save(DataModel model)
        {
            File.WriteAllText(file, model.JObject.ToString(/*specify no-indent?*/));
        }
    }
}
