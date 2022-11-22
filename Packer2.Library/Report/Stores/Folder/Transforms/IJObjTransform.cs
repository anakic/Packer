using Newtonsoft.Json.Linq;

namespace Packer2.Library.Report.Stores.Folder.Transforms
{
    interface IJObjTransform
    {
        void Transform(JObject obj);
        void Restore(JObject obj);
    }
}
