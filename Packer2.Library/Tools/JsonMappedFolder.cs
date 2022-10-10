using Newtonsoft.Json.Linq;
using Packer2.Library.Tools;

namespace Packer2.Library.Tools
{
    abstract class JsonMappedFolder
    {
        protected abstract string RootFileName { get; }

        protected abstract IEnumerable<MappingZone> Mappings { get; }

        protected string GetFilePath(string baseFolder) => Path.Combine(baseFolder, RootFileName);

        public void Write(JObject obj, string baseFolder)
        {
            foreach (var mapping in Mappings)
                mapping.Write(obj, baseFolder, string.Empty);
            var rootFilePath = GetFilePath(baseFolder);
            FileTools.WriteToFile(rootFilePath, obj);
        }

        public JObject Read(string baseFolder)
        {
            JObject jObj = JObject.Parse(File.ReadAllText(GetFilePath(baseFolder)));
            foreach (var map in Mappings)
                map.Read(jObj, baseFolder, string.Empty);
            return jObj;
        }
    }
}
