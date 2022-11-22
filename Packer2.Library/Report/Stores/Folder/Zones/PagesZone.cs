using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Stores.Folder.Zones
{
    class PagesZone : JsonElementZone
    {
        IEnumerable<MappingZone> childMappings;
        public PagesZone()
        {
            childMappings = new List<MappingZone>() { new VisualZone() };
        }

        protected override string ContainingFolder => "Pages";

        protected override string ElementsSelector => ".sections[*]";

        protected override IEnumerable<MappingZone> ChildMappings => childMappings;

        protected override string GetFileName(JToken elem) => "page";

        protected override string GetFileExtension(JToken elem) => "json";

        protected override string GetSubfolderForElement(JToken elem) => (string)elem["displayName"]!;
    }
}
