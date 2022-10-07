using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsData.Export, "TabularModelDataSources")]
    public class ExportTabularModelDataSourcesCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform()
            => new ExportDataSourcesTransform(CreateLogger<ExportDataSourcesTransform>());
    }
}
