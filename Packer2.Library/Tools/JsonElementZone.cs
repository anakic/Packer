using Newtonsoft.Json.Linq;

namespace Packer2.Library.Tools
{
    /// <remarks>
    /// Derived classes should not filter based on properties in the target objects in the ElementSelector!
    /// They must target all objects based on their location in the hierarchy and filter them using the
    /// FilterSelectedElements method instead. This is because the target objects get replaced but we still
    /// must be able to find the replacement objects
    /// </remarks>
    abstract class JsonElementZone : MappingZone
    {
        const string RefProp = "fileRef";
        const string MappingZone = "mappingZone";

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
            => elem.Replace(new JObject(new JProperty(RefProp, destinationPath), new JProperty(MappingZone, GetType().FullName)));

        protected sealed override bool FilterSelectedProcessedElements(JToken element)
        {
            var mappingZone = (string?)element[MappingZone];
            return mappingZone == GetType().FullName;
        }
    }
}
