using Newtonsoft.Json.Linq;

namespace Packer2.Library.Tools
{
    abstract class JsonPropertyZone : MappingZone
    {
        const string refPrefix = "fileRef-";

        protected sealed override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

        protected abstract string PayloadContainingObjectJsonPath { get; }

        protected abstract string GetPayloadProperty(JToken obj);

        public sealed override string GetPayload(JToken obj)
            => (string)obj.SelectToken(PayloadContainingObjectJsonPath)![GetPayloadProperty(obj)]!;

        protected sealed override JToken ApplyElementPayload(JToken elem, string payload)
        {
            var payloadProperty = GetPayloadProperty(elem);
            (elem.SelectToken(PayloadContainingObjectJsonPath)![refPrefix + payloadProperty]!.Parent as JProperty)!.Replace(new JProperty(payloadProperty, payload));
            return elem;
        }

        protected sealed override string ReadPayloadLocation(JToken elem)
        {
            var payloadProperty = GetPayloadProperty(elem);
            return (string)elem.SelectToken(PayloadContainingObjectJsonPath)?[refPrefix + payloadProperty];
        }

        protected sealed override void RegisterPayloadLocation(JToken elem, string destinationFileName)
        {
            var payloadProperty = GetPayloadProperty(elem);
            var payloadPropertyToken = elem.SelectToken(PayloadContainingObjectJsonPath)?[payloadProperty];
            if (payloadPropertyToken == null)
            {
                if (PayloadMandatory)
                    throw new Exception($"No payload found for zone {GetType().Name}, json path is '{elem.Path}'!");
                else
                    return;
            }
            else
                (payloadPropertyToken.Parent as JProperty)!.Replace(new JProperty(refPrefix + payloadProperty, destinationFileName));
        }

        // JsonPropertyZone zones do not have children so they won't create new subfolders
        protected override string GetSubfolderForElement(JToken elem) => String.Empty;

        /// <summary>
        /// If true, the payload must exist else an exception will be thrown. If the payload property is allowed to not exist and it does not it is simply skipped.
        /// </summary>
        protected virtual bool PayloadMandatory { get; } = false;
    }
}
