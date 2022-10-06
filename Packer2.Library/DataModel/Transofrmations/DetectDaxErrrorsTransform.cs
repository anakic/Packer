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
            if (database.Server == null)
                throw new Exception("Dax validation currently only available for models that are connected to an SSAS database.");

            var columnErrorMessages = database.Model.Tables.SelectMany(c => c.Columns).Where(c => !string.IsNullOrEmpty(c.ErrorMessage)).Select(c => $"{GetName(c.Parent)}[{c.Name}]: {c.ErrorMessage})");
            var measureErrorMessages = database.Model.Tables.SelectMany(t => t.Measures).Where(m => !string.IsNullOrEmpty(m.ErrorMessage)).Select(m => $"{GetName(m.Parent)}[{m.Name}]: {m.ErrorMessage})");

            foreach (var em in columnErrorMessages.Concat(measureErrorMessages))
                reportError(em);

            return database;
        }

        private string GetName(MetadataObject metadataObject)
        {
            if (metadataObject is NamedMetadataObject nmo)
                return nmo.Name;
            else
                return metadataObject.ToString();
        }
    }
}
