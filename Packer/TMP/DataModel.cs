using Newtonsoft.Json.Linq;

namespace Packer.TMP
{
    // the data model consists of a single file
    // any transformations are done on that single file
    // the file can be stored as many files (e.g. in repo)
    // or as a single file (for deployment)
    class DataModel
    {
        public DataModel(JObject jObject)
        {
            JObject = jObject;
        }

        public JObject JObject { get; }
    }
}
