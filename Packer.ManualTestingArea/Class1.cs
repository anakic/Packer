//using CsvHelper;
//using Microsoft.AnalysisServices.Tabular;
//using Packer2.Library;
//using Packer2.Library.Report.Transforms;
//using System.Globalization;

//namespace Packer.ManualTestingArea
//{
//    record UsageReport(NamedMetadataObject Object, Dictionary<string, int> ReferencesByReport);

//    record ObjectUsageReport2(string Name, string Kind, int RefCount, string Details);

//    public class Class1
//    {
//        public static void Main()
//        {
//            var db = GetModel();

//            var detectionsByReport = new Dictionary<string, Detections>();
//            foreach (var reportFolder in GetReportFolders())
//            {
//                var report = PbiReportLoader.LoadFromFolder(reportFolder);
//                Detections detections = new Detections();
//                report.DetectReferences(detections);
//                detectionsByReport.Add(reportFolder, detections);
//            }

//            var usages = new List<UsageReport>();
//            foreach (var tbl in db.Model.Tables)
//            {
//                var tblDictdict = new Dictionary<string, int>();
//                var tblUsageReport = new UsageReport(tbl, tblDictdict);
//                usages.Add(tblUsageReport);
//                int totalTableReferences = 0;
//                foreach (var kvp in detectionsByReport)
//                {
//                    var directTableReferences = kvp.Value.TableReferences.Where(tr => tr.TableName == tbl.Name);
//                    var columnsFromTableReferences = kvp.Value.ColumnReferences.Where(tr => tr.TableName == tbl.Name);
//                    var measuresFromTableReferences = kvp.Value.MeasureReferences.Where(tr => tr.TableName == tbl.Name);
//                    var totalReferences = directTableReferences.Count() + columnsFromTableReferences.Count() + measuresFromTableReferences.Count();
//                    if (totalReferences > 0)
//                        tblDictdict.Add(kvp.Key, totalReferences);
//                    totalTableReferences += totalReferences;
//                }

//                // if the table is not referenced anywhere, no use listing its children individually
//                if (totalTableReferences == 0)
//                    continue;

//                foreach (var col in tbl.Columns)
//                {
//                    var colDict = new Dictionary<string, int>();
//                    var colUsageReport = new UsageReport(col, colDict);
//                    foreach (var kvp in detectionsByReport)
//                    {
//                        var colRefsCount = kvp.Value.ColumnReferences.Where(tr => tr.TableName == tbl.Name && tr.Column == col.Name).Count();
//                        if (colRefsCount > 0)
//                            colDict.Add(kvp.Key, colRefsCount);
//                    }
//                    usages.Add(colUsageReport);

//                    // check if it's used in calc columns and measures
//                    foreach (var calcCol in tbl.Columns.OfType<CalculatedColumn>())
//                    {
//                        if (calcCol == col)
//                            continue;

//                        if (calcCol.Expression.Contains(col.Name))
//                            colDict.Add(calcCol.Name, 1);
//                    }
//                    foreach (var meas2 in tbl.Measures)
//                    {
//                        if (meas2.Expression.Contains(col.Name))
//                            colDict.Add(meas2.Name, 1);
//                    }
//                }

//                foreach (var meas in tbl.Measures)
//                {
//                    var measDict = new Dictionary<string, int>();
//                    var measUsageReport = new UsageReport(meas, measDict);
//                    foreach (var kvp in detectionsByReport)
//                    {
//                        var measRefsCount = kvp.Value.MeasureReferences.Where(tr => tr.TableName == tbl.Name && tr.Measure == meas.Name).Count();
//                        if (measRefsCount > 0)
//                            measDict.Add(kvp.Key, measRefsCount);
//                    }
//                    usages.Add(measUsageReport);

//                    // check if it's used in calc columns and measures
//                    foreach (var calcCol in tbl.Columns.OfType<CalculatedColumn>())
//                    {
//                        if (calcCol.Expression.Contains(meas.Name))
//                            measDict.Add(calcCol.Name, 1);
//                    }
//                    foreach (var meas2 in tbl.Measures)
//                    {
//                        if (meas == meas2)
//                            continue;

//                        if (meas2.Expression.Contains(meas.Name))
//                            measDict.Add(meas2.Name, 1);
//                    }
//                }
//            }


//            var usages2 = usages.Select(x =>
//            {
//                var name = x.Object.Name;

//                if (x.Object is Column c)
//                    name = $"{c.Table.Name}.{c.Name}";
//                if (x.Object is Measure m)
//                    name = $"{m.Table.Name}.{m.Name}";

//                var totalUsages = x.ReferencesByReport.Sum(y => y.Value);
//                var details = string.Join(", ", x.ReferencesByReport.Select(z => $"{z.Key} ({z.Value})"));
                
//                return new ObjectUsageReport2(name, x.Object.GetType().Name, totalUsages, details);
                
//            }).ToList();

//            using (var writer = new StreamWriter(@"c:\\work\\usages.csv"))
//            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
//            {
//                csvWriter.WriteHeader<ObjectUsageReport2>();
//                csvWriter.NextRecord();
//                csvWriter.WriteRecords(usages2);
//            }
//        }

//        private static IEnumerable<string> GetReportFolders()
//        {
//            return Directory.GetFiles("C:\\work\\DHCFT\\Modules\\DHCFT", "[Content_Types].xml", SearchOption.AllDirectories)
//                .Select(x => Path.GetDirectoryName(x)!);
//        }

//        private static Database GetModel()
//        {
//            return TabularModel.LoadFromFolder("C:\\work\\DHCFT\\Data Model");
//        }
//    }
//}
