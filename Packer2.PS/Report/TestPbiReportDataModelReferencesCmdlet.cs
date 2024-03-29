﻿using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.Report.Transforms;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsDiagnostic.Test, "PbiReportDataModelReferences")]
    public class TestPbiReportDataModelReferencesCmdlet : StoreCmdletBase
    {
        [Parameter(ValueFromPipeline = true)]
        public PowerBIReport Report { get; set; }

        [Parameter(Mandatory = false, Position = 0)]
        [Alias("dm")]
        public string DataModelLocation { get; set; }

        protected override void ProcessRecord()
        {
            var logger = CreateLogger<TestPbiReportDataModelReferencesCmdlet>();

            IModelStore<Database> dataModelStore = null;
            if (DataModelLocation != null)
            {
                logger.LogInformation("Loading model from specified location...");
                dataModelStore = GetDataModelStore(DataModelLocation);
            }
            else
            {
                if (Report.DataModelSchemaFile != null)
                {
                    logger.LogInformation("Loading model from the report's built-in DataModelSchema...");
                    dataModelStore = new BimDataModelStore(new JObjFile(Report.DataModelSchemaFile));
                }
                else
                {
                    if (Report.Connections != null)
                    {
                        foreach (var connToken in Report.Connections.SelectTokens(".Connections[*]"))
                        {
                            var type = connToken["ConnectionType"]!.ToString();
                            if (type == "analysisServicesDatabaseLive")
                            {
                                var name = connToken["Name"]!.ToString();
                                var connString = connToken["ConnectionString"]!.ToString();
                                logger.LogInformation("Using first SSAS connection found in Connections.json, connection name is '{connectionName}', connection string is '{connectionString}'.", name, connString);
                                dataModelStore = GetDataModelStore(connString);
                            }
                        }
                    }
                }
            }

            if (dataModelStore == null)
                throw new ArgumentException("Data model location was not specified, and a data model connection could not be inferred (no DataModelSchema and no existing connection in the Connections file).");

            var database = dataModelStore.Read();

            var transform = new ValidateModelReferencesTransform(database, CreateLogger<ValidateModelReferencesTransform>());
            transform.Transform(Report.Layout);

            if (transform.UnmatchedDetections.Any())
            {
                var myLogger = CreateLogger<TestPbiReportDataModelReferencesCmdlet>();
                foreach (var d in transform.UnmatchedDetections)
                {
                    switch (d)
                    {
                        case InvalidTableDetection t:
                            myLogger.LogError("Invalid table reference: {table} at path {path}", t.Table, t.Path);
                            break;
                        case InvalidColumnDetection c:
                            myLogger.LogError("Invalid column reference: {table}.{column} at path {path}", c.Table, c.Column, c.Path);
                            break;
                        case InvalidMeasureDetection m:
                            myLogger.LogError("Invalid measure reference: {table}.{measure} at path {path}", m.Table, m.Measure, m.Path);
                            break;
                        case InvalidHierarchyDetection h:
                            myLogger.LogError("Invalid hiearchy reference: {table}.{hierarchy} at path {path}", h.Table, h.Hierarchy, h.Path);
                            break;
                        case InvalidHierarchyLevelDetection hl:
                            myLogger.LogError("Invalid hiearchy reference: {table}.{hierarchy}.{level} at path {path}", hl.Table, hl.Hierarchy, hl.HierarchyLevel, hl.Path);
                            break;
                        default:
                            throw new NotImplementedException("Unexpected detection type - should never happen.");
                    }
                }

                throw new Exception($"### A total of {transform.UnmatchedDetections.Count()} invalid references found! ###");
            }

            base.ProcessRecord();

            WriteObject(Report);
        }
    }
}
