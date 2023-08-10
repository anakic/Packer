using Microsoft.AnalysisServices.Tabular;
using Newtonsoft.Json.Linq;

namespace Packer2.Library.DataModel
{
    public class BimDataModelStore : IModelStore<Database>
    {
        private readonly ITextStore file;

        public static BimDataModelStore LoadFromText(string contents)
        {
            return new BimDataModelStore(new MemoryFile(contents));
        }
        public static BimDataModelStore LoadFromFile(string filePath)
        {
            return new BimDataModelStore(new TextFileStore(filePath));
        }

        public BimDataModelStore(ITextStore file)
        {
            this.file = file;
        }

        public Database Read()
        {
            return JsonSerializer.DeserializeDatabase(file.GetText());
        }

        public void Save(Database model)
        {
            var serialized = JsonSerializer.SerializeDatabase(model, new SerializeOptions() { IgnoreInferredObjects = true, IgnoreInferredProperties = true, IgnoreTimestamps = true });

            // Git diffs were showing a bit of noise where order of some objects was inconsistent from one save to the next. Enforcing order of names.
            serialized = OrderModelJSONSection(serialized, "tables");
            serialized = OrderModelJSONSection(serialized, "relationships");
            serialized = OrderModelJSONSection(serialized, "expressions");
            serialized = OrderModelJSONSection(serialized, "annotations");

            serialized = SetDefaultConnectionDetails(serialized);

            file.SetText(serialized);
        }

        private string OrderModelJSONSection(string serialized, string sectionName, string orderByName = "name")
        {
            //Load the json into a jobject
            JObject jObject = JObject.Parse(serialized);

            //Find the tables jarray
            JArray section = (JArray)jObject["model"][sectionName];

            //Sort it by name
            JArray sortedSection = new JArray(section.OrderBy(obj => (string)obj[orderByName]));

            //Replace the original jarray with the sorted one
            jObject["model"][sectionName] = sortedSection;

            //Convert the sorted jObject back to a string
            serialized = jObject.ToString();

            return serialized;
        }

        private string SetDefaultConnectionDetails(string serialized)
        {
            //Load the json into a jobject
            JObject jObject = JObject.Parse(serialized);

            // Remove the line for model.defaultPowerBIDataSourceVersion
            if (jObject["model"]["defaultPowerBIDataSourceVersion"] != null)
            {
                jObject["model"]["defaultPowerBIDataSourceVersion"].Parent.Remove();
            }

            // For any of the model.dataSources set connectionString to a default and set connectionDetails.database to a default
            // These get overwritten at the proper build time anyway so they don't need real values stored for source control
            JArray dataSources = (JArray)jObject["model"]["dataSources"];

            if (!(dataSources is null))
            {
                foreach (JObject dataSource in dataSources)
                {
                    // If the property exists, set it to the default
                    if (dataSource["connectionString"] != null)
                    {
                        dataSource["connectionString"] = "Data Source=.;Initial Catalog=Model;Integrated Security=True";
                    }
                    if (dataSource["connectionDetails"] != null)
                    {
                        dataSource["connectionDetails"]["address"]["database"] = "Model";
                    }
                }
            }
            //Convert the sorted jObject back to a string
            serialized = jObject.ToString();

            return serialized;
        }
    }
}
