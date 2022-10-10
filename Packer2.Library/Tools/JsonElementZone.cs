using Newtonsoft.Json.Linq;

namespace Packer2.Library.Tools
{
    abstract class JsonElementZone : MappingZone
    {
        const string RefProp = "fileRef";

        public sealed override string GetPayload(JToken obj)
            => obj.ToString(Newtonsoft.Json.Formatting.Indented);

        protected sealed override JToken ApplyElementPayload(JToken elem, string payload)
        {
            var obj = JObject.Parse(payload);
            elem.Replace(obj);
            return obj;
        }

        protected sealed override string ReadPayloadLocation(JToken elem)
            => (string)elem[RefProp]!;

        protected sealed override void RegisterPayloadLocation(JToken elem, string destinationPath)
            => elem.Replace(new JObject(new JProperty(RefProp, destinationPath)));
    }
}
