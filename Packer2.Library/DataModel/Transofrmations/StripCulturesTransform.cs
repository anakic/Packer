using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel.Transofrmations
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
