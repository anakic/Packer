using Packer.Model;

namespace Packer.Steps
{
    class StepBase
    {
        public virtual void ToHumanReadable(RepositoryModel model)
        {
        }

        public virtual void ToMachineReadable(RepositoryModel model)
        {
        }
    }
}
