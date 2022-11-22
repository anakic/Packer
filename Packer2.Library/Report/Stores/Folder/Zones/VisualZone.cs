using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Stores.Folder.Zones
{
    class VisualZone : JsonElementZone
    {
        protected override string ContainingFolder => string.Empty;

        protected override string ElementsSelector => ".visualContainers[*].#config";

        protected override IEnumerable<MappingZone> ChildMappings => Array.Empty<MappingZone>();

        protected override string GetFileExtension(JToken elem) => "json";

        protected override string GetFileName(JToken elem)
        {
            if (elem["singleVisualGroup"] != null)
                return $"{elem.SelectToken("singleVisualGroup.displayName")!.ToString()} ({elem.SelectToken("name")!.ToString()})";
            else
                return elem.SelectToken("name")!.ToString();
        }

        protected override string GetSubfolderForElement(JToken elem)
        {
            if (elem["singleVisualGroup"] != null)
                return "VisualGroups";
            else
                return elem.SelectToken("singleVisual.visualType")!.ToString();
        }
    }
}
