using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class StripCulturesTransform : IDataModelTransform
    {
        private readonly ILogger<StripCulturesTransform> logger;

        public StripCulturesTransform(ILogger<StripCulturesTransform>? logger = null)
        {
            this.logger = logger ?? new DummyLogger<StripCulturesTransform>();
        }

        public Database Transform(Database database)
        {
            logger.LogInformation("Removing {nrOfCultures} culture entries", database.Model.Cultures.Count);

            database.Model.Cultures.Clear();
            return database;
        }
    }
}
