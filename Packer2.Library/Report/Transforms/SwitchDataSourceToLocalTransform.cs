using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Transofrmations;
using Packer2.Library.Tools;
using System.Xml.Linq;

namespace Packer2.Library.Report.Transforms
{
    public class SwitchDataSourceToLocalTransform : IReportTransform
    {
        private readonly string? connectionString;
        private readonly ILogger<SwitchDataSourceToSSASTransform> logger;

        public SwitchDataSourceToLocalTransform(string? connectionString, ILogger<SwitchDataSourceToSSASTransform>? logger = null)
        {
            this.logger = logger ?? new DummyLogger<SwitchDataSourceToSSASTransform>();
            this.connectionString = connectionString;
        }

        public PowerBIReport Transform(PowerBIReport reportModel)
        {
            // 1. add DataModelSchema content type
            var ns = @"http://schemas.openxmlformats.org/package/2006/content-types";
            var nodesToRemove = new[] { "/DataModelSchema" }.ToHashSet();
            logger.LogInformation("Updating [Content_Types].xml file...");
            reportModel.Content_Types.Descendants(XName.Get("Override", ns))
                .Where(xe => nodesToRemove.Contains(xe.Attribute("PartName")?.Value))
                ?.Remove();
            reportModel.Content_Types.Root!.Add(new XElement(XName.Get("Override", ns), new XAttribute("PartName", "/DataModelSchema"), new XAttribute("ContentType", "")));


            // 2. remove connection from Connections.json
            var connectionStringToUse = connectionString;
            var connArr = reportModel.Connections["Connections"] as JArray;
            if (connArr != null)
            {
                if (string.IsNullOrEmpty(connectionStringToUse))
                    connectionStringToUse = connArr.Select(j => j["ConnectionString"]).Where(x => x != null).Select(x => x.Value<string>()).SingleOrDefault();
                reportModel.Connections.Remove("Connections");
            }

            // 3. pack datamodel into the report
            var modelStore = new SSASDataModelStore(connectionStringToUse);
            var dataModel = modelStore.Read();

            // strip out all partitions except the first one in each table (local model does not support multiple partitions)
            dataModel = new ClearSecondaryPartitionsTransform().Transform(dataModel);

            var mf = new JObjFile();
            var bimModelStore = new BimDataModelStore(mf);
            bimModelStore.Save(dataModel);

            reportModel.DataModelSchemaFile = mf.JObject;

            return reportModel;
        }
    }
}
