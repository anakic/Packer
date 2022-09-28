using Microsoft.AnalysisServices.Tabular;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class PasswordResolveTransform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            foreach (var cred in database.Model.DataSources.OfType<StructuredDataSource>().Select(ds => ds.Credential))
            {
                var pwd = cred.Password;
                var m = Regex.Match(pwd, @"{(?'name'[^}]+)}");
                if (m.Success)
                {
                    var value = Environment.GetEnvironmentVariable(m.Groups["name"].Value);
                    if (value != null)
                    {
                        cred.Password = value;
                    }
                }
            }
            return database;
        }
    }
}
