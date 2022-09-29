using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class MergeDataSourcesTransform : IDataModelTransform
    {
        private readonly Database source;

        public MergeDataSourcesTransform(Database source)
        {
            this.source = source;
        }

        public Database Transform(Database database)
        {
            foreach (var ds in source.Model.DataSources)
            {
                if(database.Model.DataSources.Contains(ds.Name))
                    database.Model.DataSources.Remove(ds.Name);
                database.Model.DataSources.Add(ds.Clone());
            }

            return database;
        }
    }
}