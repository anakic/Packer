using Newtonsoft.Json.Linq;
using System.Text;

namespace Packer.Steps
{
    internal class StripTimestapsStep : StepBase
    {
        HashSet<string> propertiesToStrip = new HashSet<string>() { "createdTimestamp", "modifiedTime", "structureModifiedTime", "refreshedTime", "lastUpdate", "lastSchemaUpdate", "lastProcessed" };

        public override void Extract(string pbitFilePath, string folderPath)
        {
            // strip timestamps
            var dataModelSchemaFile = Path.Combine(folderPath, "DataModelSchema");
            var jObj = JObject.Parse(File.ReadAllText(dataModelSchemaFile));
            jObj.Descendants()
                .OfType<JProperty>()
                .Where(jp => propertiesToStrip.Contains(jp.Name))
                .ToList().ForEach(jp => jp.Remove());
            var bytes = Encoding.Unicode.GetBytes(jObj.ToString());
            File.WriteAllBytes(dataModelSchemaFile, bytes);

            base.Extract(pbitFilePath, folderPath);
        }
    }
}
