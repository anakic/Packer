using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    /// <summary>
    /// Stripts timestamps from the DataModelSchema file. These change when edited but they are not useful code
    /// and would pollute the diff with information that is not relevant to us. PowerBI also doesn't mind if we
    /// strip them out.
    /// </summary>
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
