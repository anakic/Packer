using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsData.Import, "TabularModelDataSources")]
    public class ImportTabularModelDataSourcesCmdlet : DataModelTransformCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("s")]
        public string SourceModel { get; set; }

        protected override IDataModelTransform CreateTransform()
        {
            var sourceDb = GetDataModelStore(SourceModel).Read();
            return new MergeDataSourcesTransform(sourceDb);
        }
    }
}
