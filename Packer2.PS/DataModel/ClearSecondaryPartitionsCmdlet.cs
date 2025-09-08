using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommon.Clear, "SecondaryPartitions")]
    public class ClearSecondaryPartitionsCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform()
            => new ClearSecondaryPartitionsTransform();
    }
}
