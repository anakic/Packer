using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommon.Remove, "TabularModelAutoTimeIntelligence")]
    public class RemoveTabularModelAutoTimeIntelligenceCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform()
            => new StripLocalDateTablesTransform();
    }
}
