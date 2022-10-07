using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommon.Switch, "TabularModelCompatibilityLevel")]
    public class SwitchTabularModelCompatibilityLevelCmdlet : DataModelTransformCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("ver", "v")]
        public int Version { get; set; }

        protected override IDataModelTransform CreateTransform()
            => new DowngradeTransform(Version, CreateLogger<DowngradeTransform>());
    }
}
