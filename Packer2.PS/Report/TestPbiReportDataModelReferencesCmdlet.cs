using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.Report.Transforms;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsDiagnostic.Test, "PbiReportDataModelReferences")]
    public class TestPbiReportDataModelReferencesCmdlet : StoreCmdletBase
    {
        [Parameter(ValueFromPipeline = true)]
        public PowerBIReport Report { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        [Alias("dm")]
        public string DataModelLocation { get; set; }

        protected override void ProcessRecord()
        {
            IModelStore<Database> dataModelStore;
            if (DataModelLocation != null)
                dataModelStore = GetDataModelStore(DataModelLocation);
            else
                dataModelStore = new BimDataModelStore(new JObjFile(Report.DataModelSchemaFile));

            var database = dataModelStore.Read();

            var transform = new ValidateModelReferencesTransform(database, CreateLogger<ValidateModelReferencesTransform>());
            transform.Transform(Report);

            base.ProcessRecord();

            WriteObject(Report);
        }
    }
}
