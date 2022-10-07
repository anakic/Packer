using DataModelLoader.Report;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Xml.Linq;

namespace Packer2.Library.Report.Transforms
{
    public class SwitchDataSourceToSSASTransform : IReportTransform
    {
        private readonly string? connectionString;

        public SwitchDataSourceToSSASTransform(string? connectionString = null)
        {
            this.connectionString = connectionString;
        }

        public PowerBIReport Transform(PowerBIReport model)
        {
            // remove data model schema (for .pbix files)
            model.DataModelSchemaFile = null;

            var stream = typeof(SwitchDataSourceToSSASTransform).Assembly.GetManifestResourceStream("Packer2.Library.Resources.DataMashup");
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                model.Blobs["DataMashup"] = memoryStream.ToArray();
            }

            var ns = @"http://schemas.openxmlformats.org/package/2006/content-types";

            var nodesToRemove = new[] { "/DataModelSchema", "/DataModel", "/DataMashup", "/Connections" }.ToHashSet();
            model.Content_Types.Descendants(XName.Get("Override", ns))
                .Where(xe => nodesToRemove.Contains(xe.Attribute("PartName")?.Value))
                ?.Remove();

            model.Content_Types.Root!.Add(new XElement(XName.Get("Override", ns), new XAttribute("PartName", "/Connections"), new XAttribute("ContentType", "")));
            model.Content_Types.Root!.Add(new XElement(XName.Get("Override", ns), new XAttribute("PartName", "/DataMashup"), new XAttribute("ContentType", "")));

            if (connectionString != null)
            {
                if (model.Connections == null)
                    model.Connections = JObject.FromObject(new { Version = 3 });

                model.Connections["Connections"] =
                    new JArray(
                        new JObject(
                                new JProperty("Name", "EntityDataSource"),
                                new JProperty("ConnectionString", new SqlConnectionStringBuilder(connectionString).ConnectionString /* this will convert "Server" to "Data source" and "Database" to "Initial catalog" which seems to be required by powerbi*/),
                                new JProperty("ConnectionType", "analysisServicesDatabaseLive")
                        )
                );
            }

            return model;
        }

    }
}
