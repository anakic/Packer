using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsLifecycle.Register, "TabularModelDataSources")]
    public class RegisterTabularModelDataSourceCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform()
            => new RegisterDataSourcesTransform(CreateLogger<RegisterDataSourcesTransform>());
    }
}
