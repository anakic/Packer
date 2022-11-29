using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class ExportDataSourcesTransform : IDataModelTransform
    {
        private readonly ILogger<ExportDataSourcesTransform> logger;

        public ExportDataSourcesTransform(ILogger<ExportDataSourcesTransform>? logger = null)
        {
            this.logger = logger ?? new DummyLogger<ExportDataSourcesTransform>();
        }

        public Database Transform(Database database)
        {
            var extractDb = new Database()
            {
                Name = "Data sources",
                CompatibilityLevel = 1400,
                Model = new Model()
            };

            foreach (var ds in database.Model.DataSources)
            {
                logger.LogInformation("Including data source {dataSource}", ds.Name);
                extractDb.Model.DataSources.Add(ds.Clone());
            }

            return extractDb;
        }
    }
}
