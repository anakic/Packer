using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class ExportDataSourcesTransform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            var extractDb = new Database()
            {
                Name = "Data sources",
                CompatibilityLevel = 1400,
                Model = new Model()
            };

            foreach (var ds in database.Model.DataSources)
                extractDb.Model.DataSources.Add(ds.Clone());

            return extractDb;
        }
    }
}
