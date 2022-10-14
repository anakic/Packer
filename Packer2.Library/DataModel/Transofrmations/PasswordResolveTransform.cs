using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.Tools;
using System.Text.RegularExpressions;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class PasswordResolveTransform : IDataModelTransform
    {
        Func<string, string> promptForInputFunc;

        private readonly ILogger<PasswordResolveTransform> logger;

        public PasswordResolveTransform(Func<string, string> promptForInputFunc, ILogger<PasswordResolveTransform> logger = null)
        {
            this.promptForInputFunc = promptForInputFunc;
            this.logger = logger ?? new DummyLogger<PasswordResolveTransform>();
        }

        public Database Transform(Database database)
        {
            foreach (var ds in database.Model.DataSources.OfType<StructuredDataSource>())
            {
                var pwd = ds.Credential.Password;
                var m = Regex.Match(pwd, @"{(?'name'[^}]+)}");
                if (m.Success)
                {
                    var placeholder = m.Groups["name"].Value;
                    var value = Environment.GetEnvironmentVariable(placeholder);
                    
                    if (value == null)
                    {
                        logger.LogInformation("Found password placeholder '{passwordPlaceholder}' in data source '{dataSource}' but a matching environment variable has not been found. Prompting for password.", placeholder, ds.Name);
                        value = promptForInputFunc(placeholder);
                    }

                    logger.LogInformation("Filling password placeholder '{passwordPlaceholder}' in data source '{dataSource}'", placeholder, ds.Name);
                    ds.Credential.Password = value;
                }
            }
            return database;
        }
    }
}
