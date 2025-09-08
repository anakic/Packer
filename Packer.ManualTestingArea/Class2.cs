using Antlr4.Runtime;
using DataModelLoader.Report;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.Report.Stores.Folder;
using Packer2.Library.Report.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.ManualTestingArea
{
    enum ObjectKind
    {
        Table,
        Column,
        Measure,
    }

    enum ReportReferenceKind
    {
        Data,
        Filter
    }

    record ReportReference(string ObjectName, string Page, string VisualId, string Query, ReportReferenceKind ReportReferenceKind);

    record DataModelReference(string ObjectName, string ReferencingExpression);

    record ReferenceReport(string ObjectName, ObjectKind ObjectKind, List<Detections> ReportReferences, List<DataModelReference> DataModelReferences);

    public class Class2
    {
        public static void Main()
        {
            var reportStore = new PBIArchiveStore("C:\\merging\\Fast4.pbit");
            var report = reportStore.Read();

            var dataModelStore = new BimDataModelStore(new MemoryFile(report.DataModelSchemaFile!.ToString()));
            var dataModel = dataModelStore.Read();

            var detections = new Detections();
            report.DetectReferences(detections);


            List<string> measures = new();
            foreach (var t in dataModel.Model.Tables)
            {
                foreach (var c in t.Columns)
                { }

                foreach (var m in t.Measures)
                {
                    var measureReferences = detections.MeasureReferences
                        .Where(mr => mr.TableName == t.Name && mr.Measure == m.Name)
                        .Select(mr => new ReportReference(mr.Measure, /*mr.Page, mr.VisualId, mr.Query,*/null,null,null, ReportReferenceKind.Data))
                        .ToList();
                    
                    if(measureReferences.Count == 0)
                        measures.Add(m.Name);

                    //var referenceReport = new ReferenceReport(m.Name, ObjectKind.Measure, measureReferences, dataModelReferences);
                    //Console.WriteLine(referenceReport);
                }
            }
        }
    }
}
