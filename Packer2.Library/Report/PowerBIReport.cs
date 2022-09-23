using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Transforms;
using System.Xml.Linq;

namespace DataModelLoader.Report
{
    public class PowerBIReport
    {
        public JObject? DataModelSchemaFile { get; set; }
        public JObject DiagramLayout { get; set; }
        public JObject Metadata { get; set; }
        public JObject Settings { get; set; }
        public JObject Connections { get; set; }

        public XDocument Report_LinguisticSchema { get; set; }
        public XDocument Content_Types { get; set; }

        public string Version { get; set; }

        public Dictionary<string, byte[]> Blobs { get; } = new Dictionary<string, byte[]>();

        public JObject Layout { get; set; }
    }
}
