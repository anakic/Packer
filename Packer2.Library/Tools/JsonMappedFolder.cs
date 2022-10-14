using Newtonsoft.Json.Linq;
using Packer2.FileSystem;

namespace Packer2.Library.Tools
{
    abstract class JsonMappedFolder
    {
        protected abstract string RootFileName { get; }

        protected abstract IEnumerable<MappingZone> Mappings { get; }

        public void Write(JObject obj, IFileSystem fileSystem)
        {
            foreach (var mapping in Mappings)
                mapping.Write(obj, fileSystem, string.Empty);
            fileSystem.Save(RootFileName, obj.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public JObject Read(IFileSystem fileSystem)
        {
            JObject jObj = JObject.Parse(fileSystem.ReadAsString(RootFileName));
            foreach (var map in Mappings)
                map.Read(jObj, fileSystem, string.Empty);
            return jObj;
        }
    }
}
