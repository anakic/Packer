using Packer.Model;

namespace Packer.Steps
{
    class StepBase
    {
        public StepBase? Next { get; set; }

        public virtual void ToHumanReadable(RepositoryModel model)
        {
            Next?.ToHumanReadable(model);
        }

        public virtual void Pack(RepositoryModel model)
        {
            Next?.Pack(model);
        }
    }
}
