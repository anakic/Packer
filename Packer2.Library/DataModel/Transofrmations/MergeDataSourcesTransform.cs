using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class MergeDataSourcesTransform : IDataModelTransform
    {
        private readonly Database source;
        private readonly ILogger<MergeDataSourcesTransform> logger;

        public MergeDataSourcesTransform(Database source, ILogger<MergeDataSourcesTransform> logger = null)
        {
            this.source = source;
            this.logger = logger ?? new DummyLogger<MergeDataSourcesTransform>();
        }

        public Database Transform(Database database)
        {
            foreach (var ds in source.Model.DataSources)
            {
                if (database.Model.DataSources.Contains(ds.Name))
                {
                    database.Model.DataSources.Remove(ds.Name);
                    logger.LogInformation("Replacing data source {dataSource}", ds.Name);
                }
                else
                    logger.LogInformation("Importing new data source {dataSource}", ds.Name);
                database.Model.DataSources.Add(ds.Clone());
            }

            return database;
        }
    }
}