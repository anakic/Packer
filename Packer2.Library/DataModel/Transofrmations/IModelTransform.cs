using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel.Transofrmations
{
    public interface IDataModelTransform
    {
        Database Transform(Database database);
    }
}
