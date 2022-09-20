﻿using DataModelLoader.Report;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace Packer2.Library.Report.Transforms
{
    public interface IReportTransform
    {
        PowerBIReport Transform(PowerBIReport model);
    }

    public class RedirectToSSASTransform : IReportTransform
    {
        private readonly string connectionString;

        public RedirectToSSASTransform(string connectionString = null)
        {
            this.connectionString = connectionString;
        }

        public PowerBIReport Transform(PowerBIReport model)
        {
            model.DataModelSchemaFile = null;

            var stream = typeof(RedirectToSSASTransform).Assembly.GetManifestResourceStream("Packer2.Library.Resources.DataMashup");
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                model.Blobs["DataMashup"] = memoryStream.ToArray();
            }

            var ns = @"http://schemas.openxmlformats.org/package/2006/content-types";

            model.Content_Types.Descendants(XName.Get("Override", ns))
                .SingleOrDefault(xe => xe.Attribute("PartName")?.Value == "/DataModelSchema")
                ?.Remove();

            model.Content_Types.Root!.Add(new XElement(XName.Get("Override", ns), new XAttribute("PartName", "/DataMashup"), new XAttribute("ContentType", "")));

            if (connectionString != null)
            {
                model.Connections.Add(
                    new JProperty("Connections",
                        new JArray(
                                new JObject(
                                        new JProperty("Name", "EntityDataSource"),
                                        new JProperty("ConnectionString", connectionString),
                                        new JProperty("ConnectionType", "analysisServicesDatabaseLive")
                                )
                        )
                    )
                );
            }

            return model;
        }

    }
}
