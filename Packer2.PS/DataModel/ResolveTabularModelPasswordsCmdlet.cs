using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace Packer2.PS.DataModel
{
    [Cmdlet("Resolve", "TabularModelPasswords")]
    public class ResolveTabularModelPasswordsCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform()
            => new PasswordResolveTransform(s => 
            {
                FieldDescription fd = new FieldDescription(s);
                var res = Host.UI.Prompt("Please provide input for password placeholder", null, new System.Collections.ObjectModel.Collection<FieldDescription>(new List<FieldDescription> { fd }));
                return res[fd.Name].BaseObject.ToString();

            }, CreateLogger<PasswordResolveTransform>());
    }
}
