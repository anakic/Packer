using Newtonsoft.Json.Linq;

namespace Packer2.Library.Tools
{
    abstract class JsonPropertyZone : MappingZone
    {
        const string refPrefix = "fileRef-";

        protected sealed override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

        protected abstract string PayloadContainingObjectJsonPath { get; }

        protected abstract string PayloadProperty { get; }

        public sealed override string GetPayload(JToken obj)
            => (string)obj.SelectToken(PayloadContainingObjectJsonPath)![PayloadProperty]!;

        protected sealed override JToken ApplyElementPayload(JToken elem, string payload)
        {
            (elem.SelectToken(PayloadContainingObjectJsonPath)![refPrefix + PayloadProperty]!.Parent as JProperty)!.Replace(new JProperty(PayloadProperty, payload));
            return elem;
        }

        protected sealed override string ReadPayloadLocation(JToken elem)
        {
            return (string)elem.SelectToken(PayloadContainingObjectJsonPath)?[refPrefix + PayloadProperty];
        }

        protected sealed override void RegisterPayloadLocation(JToken elem, string destinationFileName)
        {
            var payloadProperty = elem.SelectToken(PayloadContainingObjectJsonPath)?[PayloadProperty];
            if (payloadProperty == null)
            {
                if (PayloadMandatory)
                    throw new Exception($"No payload found for zone {GetType().Name}, json path is '{elem.Path}'!");
                else
                    return;
            }
            else
                (payloadProperty.Parent as JProperty)!.Replace(new JProperty(refPrefix + PayloadProperty, destinationFileName));
        }

        // JsonPropertyZone zones do not have children so they won't create new subfolders
        protected override string GetSubfolderForElement(JToken elem) => String.Empty;

        /// <summary>
        /// If true, the payload must exist else an exception will be thrown. If the payload property is allowed to not exist and it does not it is simply skipped.
        /// </summary>
        protected virtual bool PayloadMandatory { get; } = false;
    }
}
