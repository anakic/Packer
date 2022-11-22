using Packer2.Library.Report.Stores.Folder.Zones;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Stores.Folder
{
    class ReportMappedFolder : JsonMappedFolder
    {
        protected override string RootFileName => "layout.json";

        protected override IEnumerable<MappingZone> Mappings { get; }

        public ReportMappedFolder()
        {
            Mappings = new MappingZone[]
            {
                new PagesZone(),
                new BookmarkZone(),
                new ChildBookmarkZone()
            };
        }
    }
}
