using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet("Resolve", "TabularModelPasswords")]
    public class ResolveTabularModelPasswordsCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform()
            => new PasswordResolveTransform();
    }
}
