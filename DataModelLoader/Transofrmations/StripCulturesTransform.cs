using Microsoft.AnalysisServices.Tabular;

namespace DataModelLoader.Transofrmations
{
    public class StripCulturesTransform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            database.Model.Cultures.Clear();
            return database;
        }
    }
}
