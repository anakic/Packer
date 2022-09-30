using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Transofrmations;
using Packer2.Library.Report.Transforms;
using Packer2.PS.DataModel;
using System.Linq;
using System.Management.Automation;

namespace Packer2.PS.Composite
{
    [Cmdlet("Migrate", "DataModelToSSAS")]
    public class MigrateDataModelToSSASCmdlet : StoreCmdletBase
    {
        [Parameter(Position = 0, Mandatory = false)]
        public string? ReportLocation { get; set; }

        [Parameter]
        [Alias("mo")]
        public string? ModelCodeOutputPath { get; set; }

        [Parameter]
        [Alias("ro")]
        public string? ReportOutputPath { get; set; }

        [Parameter]
        [Alias("v")]
        public int? SsasVersion { get; set; }

        [Parameter]
        [Alias("s")]
        public string? SsasServer { get; set; }

        [Parameter, Alias("d")]
        public bool DeployDatabaseToSSAS { get; set; }

        [Parameter]
        [Alias("db")]
        public string? SsasDatabaseName { get; set; }

        [Parameter]
        [Alias("rati")]
        public bool RemoveAutoTimeIntelligence { get; set; }

        [Parameter]
        [Alias("rc")]
        public bool RemoveCultures { get; set; }

        protected override void ProcessRecord()
        {
            IModelStore<PowerBIReport> reportStore = GetReportStore(ReportLocation);

            Lazy<Server> lazyServer = new Lazy<Server>(() => 
            {
                var srv = new Server();
                srv.Connect(SsasServer);
                return srv;
            });

            int version;
            if(SsasVersion.HasValue)
                version = SsasVersion.Value;
            else
            {
                
                version = lazyServer.Value.SupportedCompatibilityLevels.Split(",").Where(x => x.Length == 4/*removing the 100000 entry*/).Select(Int32.Parse).Max();
            }

            var reportModel = reportStore.Read();

            var dataModelStore = new BimDataModelStore(new MemoryFile(reportModel.DataModelSchemaFile.ToString()));
            var dataModel = dataModelStore.Read();
            IEnumerable<IDataModelTransform> dataTransforms = new IDataModelTransform[] 
            {
                new RegisterDataSourcesTransform(),
                new PullUpExpressionsTranform(),
                new DowngradeTransform(version),
            };

            if (RemoveAutoTimeIntelligence)
                dataTransforms = dataTransforms.Append(new StripLocalDateTablesTransform());

            if (RemoveCultures)
                dataTransforms = dataTransforms.Append(new StripCulturesTransform());

            foreach (var t in dataTransforms)
                dataModel = t.Transform(dataModel);

            // save code to folder
            if (ModelCodeOutputPath != null)
            {
                var outputDataModelStore = GetDataModelStore(ModelCodeOutputPath);
                outputDataModelStore.Save(dataModel);
            }

            // deploy ssas instance
            if (SsasServer != null && DeployDatabaseToSSAS)
            {
                var serverStore = new SSASDataModelStore(SsasServer, SsasDatabaseName);
                serverStore.Save(dataModel);
            }

            // remove the datamodelschema from the powerbi file and add the SSAS connection
            string? connectionString = null;
            if (SsasServer != null)
                connectionString = $"Data source = {SsasServer};";
            if (SsasDatabaseName != null)
                connectionString += $"Initial catalog={SsasDatabaseName}";
            var reportTransforms = new IReportTransform[]
            {
                new RedirectToSSASTransform(connectionString)
            };

            foreach (var t in reportTransforms)
            {
                reportModel = t.Transform(reportModel);
            }

            var outputStore = reportStore;
            if (!string.IsNullOrEmpty(ReportOutputPath))
                outputStore = GetReportStore(ReportOutputPath);

            outputStore.Save(reportModel);

            base.ProcessRecord();
        }
    }
}
