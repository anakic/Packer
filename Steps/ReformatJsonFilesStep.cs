using Newtonsoft.Json;
using Packer.Model;

namespace Packer.Steps
{
    internal class ReformatJsonFilesStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            SetFormat(model, Formatting.Indented);

            base.ToHumanReadable(model);
        }

        public override void Pack(RepositoryModel model)
        {
            base.Pack(model);

            SetFormat(model, Formatting.None);
        }

        private static void SetFormat(RepositoryModel model, Formatting formatting)
        {
            model.GetAllJsonFiles()
                .ToList().ForEach(f => f.Formatting = formatting);
        }
    }
}
