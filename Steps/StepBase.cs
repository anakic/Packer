using Packer.Model;

namespace Packer.Steps
{
    class StepBase
    {
        public StepBase? Next { get; set; }

        public virtual void Extract(RepositoryModel model)
        {
            Next?.Extract(model);
        }

        public virtual void Pack(RepositoryModel model)
        {
            Next?.Pack(model);
        }
    }
}
