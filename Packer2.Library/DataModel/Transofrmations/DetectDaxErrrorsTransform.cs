using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class DetectDaxErrrorsTransform : IDataModelTransform
    {
        private readonly Action<string> reportError;

        public DetectDaxErrrorsTransform(Action<string> reportError)
        {
            this.reportError = reportError;
        }

        public Database Transform(Database database)
        {
            var columnErrorMessages = database.Model.Tables.SelectMany(c => c.Columns).Select(c => c.ErrorMessage);
            var measureErrorMessages = database.Model.Tables.SelectMany(t => t.Measures).Select(m => m.ErrorMessage);

            foreach (var em in columnErrorMessages.Concat(measureErrorMessages))
                reportError(em);

            return database;
        }
    }
}
