using Newtonsoft.Json.Linq;
using Packer.Model;
using System.Text;

namespace Packer.Steps
{
    internal class StripTimestapsStep : StepBase
    {
        HashSet<string> propertiesToStrip = new HashSet<string>() { "createdTimestamp", "modifiedTime", "structureModifiedTime", "refreshedTime", "lastUpdate", "lastSchemaUpdate", "lastProcessed" };

        public override void ToHumanReadable(RepositoryModel model)
        {
            // strip timestamps
            model.DataModelSchemaFile!.JObj.Descendants()
                .OfType<JProperty>()
                .Where(jp => propertiesToStrip.Contains(jp.Name))
                .ToList().ForEach(jp => jp.Remove());

            base.ToHumanReadable(model);
        }
    }
}
