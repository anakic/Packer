using Packer.Model;

namespace Packer.Steps
{
    /// <summary>
    /// Base class for operations done on files in the repository when extracting/packing.
    /// </summary>
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
