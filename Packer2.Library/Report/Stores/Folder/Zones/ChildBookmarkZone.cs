using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Stores.Folder.Zones
{
    class ChildBookmarkZone : JsonElementZone
    {
        protected override string ContainingFolder => "Bookmarks";

        protected override string ElementsSelector => ".#config.bookmarks[*].children[*]";

        protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

        protected override string GetFileExtension(JToken elem) => "json";

        protected override string GetFileName(JToken elem) => (string)elem["displayName"]!;

        protected override string GetSubfolderForElement(JToken elem)
            => elem.Parent.Parent.Parent["displayName"].ToString();
    }
}
