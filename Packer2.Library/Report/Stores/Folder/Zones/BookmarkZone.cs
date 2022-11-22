using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Stores.Folder.Zones
{
    class BookmarkZone : JsonElementZone
    {
        protected override string ContainingFolder => "Bookmarks";

        // how to find bookmarks in original json
        protected override string ElementsSelector => ".#config.bookmarks[*]";

        // only target bookmarks that do not have children
        protected override bool FilterSelectedElements(JToken element)
            => element["children"] == null;

        protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

        protected override string GetFileExtension(JToken elem) => "json";

        protected override string GetFileName(JToken elem) => (string)elem["displayName"]!;

        protected override string GetSubfolderForElement(JToken elem) => string.Empty;
    }
}
