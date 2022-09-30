using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommon.Find, "TabularModelDaxErrors")]
    public class FindTabularModelDaxErrorsCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform()
            => new DetectDaxErrrorsTransform(err => WriteInformation(err, Array.Empty<string>()));
    }
}
