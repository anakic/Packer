using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class MergeDataSourcesTransform : IDataModelTransform
    {
        private readonly ITextFile sourceFile;

        public MergeDataSourcesTransform(ITextFile sourceFile)
        {
            this.sourceFile = sourceFile;
        }

        public Database Transform(Database database)
        {
            var db = JsonSerializer.DeserializeDatabase(sourceFile.GetText());

            foreach (var ds in db.Model.DataSources)
            {
                if(database.Model.DataSources.Contains(ds.Name))
                    database.Model.DataSources.Remove(ds.Name);
                database.Model.DataSources.Add(ds.Clone());
            }

            return database;
        }
    }
}