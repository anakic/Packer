using Microsoft.AnalysisServices.Tabular;
using System.Collections.Generic;

namespace DataModelLoader.Transofrmations
{
    public interface IDataModelTransform
    {
        Database Transform(Database database);
    }
}
