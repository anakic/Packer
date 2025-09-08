using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class ClearSecondaryPartitionsTransform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            foreach (var t in database.Model.Tables)
            {
                // strip out all except the fist partition
                while (t.Partitions.Count > 1)
                {
                    t.Partitions.Remove(t.Partitions[1]);
                }
            }

            return database;
        }
    }
}
