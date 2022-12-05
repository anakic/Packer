// See https://aka.ms/new-console-template for more information
using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Packer2.Library;
using Packer2.Library.Report.Stores.Folder;
using Packer2.Library.Report.Stores.Folder.Transforms;
using Packer2.Library.Report.Transforms;
using Packer2.Library.Tools;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using static System.Reflection.Metadata.BlobBuilder;
using System.Text;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter(null, LogLevel.Trace)
        .AddConsole();
});

// todo:
// 1. find unused database objects
//  - check dax (checking M not needed if tables can't reference each other but only expressions)
//  - check report references (infonav)
// 2. ability to rename measures without mentioning table name
// 3. abiltiy to move measure from table to table
// 4. ability to fix up measure parent table automatically (will fix all measures that have been moved)

var detections = new Detections();

string originalArchive = @"C:\Dropbox (RwHealth)\WLT\Flow Tool - WLT\Modules\Service Deep Dive, EIP\Service Deep Dive, EIP (WLT).pbix";
string name = Path.GetFileNameWithoutExtension(originalArchive);
string repoFolder = Path.Combine(@"c:\test\wlt", name);
string outputArchivePath = Path.Combine(@"c:\test\wlt", name + ".pbix");


//var str = File.ReadAllText(@"C:\TEst\DataModelSchema.back.json", Encoding.Unicode);
//File.WriteAllText(@"C:\TEst\DataModelSchema.back.json", str);


var rpt = PbiReportLoader.LoadFromPbiArchive(@"C:\test\Demo0.pbix");
rpt.SwitchToSSASDataSource("server=LTP-220118-ANA\\RC2022;database=test123");
rpt.SwitchToLocalDataModel();
// rpt.DataModelSchemaFile = JObject.Parse(File.ReadAllText(@"C:\TEst\znakovi.json"));
rpt.SaveToPbiArchive(@"c:\test\demo_backfromssas.pbit");

Process.Start("explorer.exe", "\"" + @"c:\test\demo_backfromssas.pbit" + "\"");


return;



//PbiReportLoader.LoadFromPbiArchive(@"c:\test\wlt\First referrals OA.pbix")
//    .SaveToFolder(@"c:\test\wlt\First referrals OA");

//PbiReportLoader
//    .LoadFromFolder(@"c:\test\wlt\First referrals OA")
//    .SetSSASDataSource(@"Server=localhost\rc2022;Database=test123");
//    .SaveToPbiArchive(@"c:\test\wlt\First referrals OA.pbix");

var model = PbiReportLoader.LoadFromPbiArchive(originalArchive);

model.ReplaceTableReference("flow ReferralSourcesSimplified", "Pathways");
model.ReplaceTableReference("Care Changes", "Flows");
model.ReplaceTableReference("Acuity Tier", "Acuity Tier (T0)");
model.ReplaceTableReference("Acuity Tier (T+1 Service)", "Acuity Tier (T+1)");
model.ReplaceTableReference("Acuity Tier (T+2 Service)", "Acuity Tier (T+2)");
model.ReplaceTableReference("Care Changes data", "Flows");
model.ReplaceTableReference("Acuity Tier (Previous Service)", "Acuity Tier (T-1)");
model.ReplaceTableReference("Acuity Tier (Previous)", "Acuity Tier (T-1)");
model.ReplaceTableReference("Acuity Tier (T-2 Service)", "Acuity Tier (T-2)");
model.ReplaceTableReference("Acuity Tier (Next Service Within 6 months)", "Acuity Tier (T+1_w6M)");
model.ReplaceTableReference("GPs", "GP Practices");
model.ReplaceTableReference("Days of Week", "Weekdays");
model.ReplaceTableReference("WardLeave", "Ward Leave");

model.ReplaceModelReference("Flows", "Service Flows for graphing", "m_Flows");
model.ReplaceModelReference("Crisis", "m_CountCrisisPresentation", "m_CrisisPresentations_Count");
model.ReplaceModelReference("Acuity Tier (T-1)", "Service/Ward", "Service");
model.ReplaceModelReference("Acuity Tier (T-2)", "Service/Ward", "Service");
model.ReplaceModelReference("Acuity Tier (T+1)", "Service/Ward", "Service");
model.ReplaceModelReference("Acuity Tier (T+2)", "Service/Ward", "Service");
model.ReplaceModelReference("Acuity Tier (T-1)", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Acuity Tier (T-2)", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Acuity Tier (T+1)", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Acuity Tier (T+2)", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Acuity Tier (T+1_w6M)", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Local Authorities", "Local Authority Name", "Local Authority");
model.ReplaceModelReference("Referral Spells", "m_CountReferralsSpells", "m_ReferralSpellsOpening_Count");
model.ReplaceModelReference("Referral Spells", "m_ReferralSpells_AverageCaseLength_Days", "m_ReferralSpells_AverageLength_Days");
model.ReplaceModelReference("Trial Balance", "m_WTEs", "m_WTEs_AllPaid");
model.ReplaceModelReference("Trial Balance", "m_Cost_Pay_PerWTE_InPeriod", "m_Cost_PerWTE_AllPaid_InPeriod");
model.ReplaceModelReference("Trial Balance", "m_Cost_Pay_PerContact", "m_Cost_PerContact");
model.ReplaceModelReference("Trial Balance", "m_Cost_Pay_PerCase", "m_CostPerReferral");
model.ReplaceModelReference("Trial Balance", "m_Cost_Pay_1000s", "m_Cost_1000s");
model.ReplaceModelReference("Trial Balance", "Cost Centre Desc", "Cost Centre Code Desc");
model.ReplaceModelReference("Acuity Tier (T0)", "Service/Ward", "Service");
model.ReplaceModelReference("Acuity Tier (T0)", "Service/Ward (Trial Balance LoD)", "Service (Trial Balance LoD)");
model.ReplaceModelReference("Acuity Tier (T0)", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Calendar", "Is Current 3 Years", "Is Last 3 Years");
model.ReplaceModelReference("Pathways", "Referral Source Simplified", "Pathway Referral source");
model.ReplaceModelReference("Calendar", "m_DatesSelected", "DateRangeHeader");
model.ReplaceModelReference("Care Changes", "Service Flows for graphing", "m_Flows_InPeriod");
model.ReplaceModelReference("Acuity Tier", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Acuity Tier (T+1 Service)", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Acuity Tier (T+2 Service)", "Simplified Service Type", "Service Type");
model.ReplaceModelReference("Localities", "Locality", "Local Authority");
model.ReplaceTableReference("Localities", "Local Authorities");
model.ReplaceTableReference("Latest Week", "Last Model Date");
model.ReplaceModelReference("Last Model Date", "Model Update Date", "m_LastUpdated_VisualLabel");
model.SaveToFolder(repoFolder);
model.SwitchToSSASDataSource(@"Server=localhost\rc2022;Database=test123");

model.SaveToPbiArchive(outputArchivePath);
model.DetectReferences(detections);

var uniqueColumns = detections.ColumnReferences.GroupBy(c => new { c.TableName, c.column }).ToList();
var uniqueMeasures = detections.MeasureReferences.GroupBy(c => new { c.TableName, c.measure }).ToList();

var uniqueTables = detections.ColumnReferences.Select(c => c.TableName).Union(detections.MeasureReferences.Select(m => m.TableName)).ToHashSet();
var db = TabularModel.LoadFromSSAS(@"localhost\rc2022", "test123");
var serverTables = db.Model.Tables.Select(t => t.Name).ToHashSet();

var unmappedTables = uniqueTables.Except(serverTables).ToList();

Console.WriteLine("Unmapped tables:");
Console.WriteLine("================");
foreach (var t in unmappedTables)
    Console.WriteLine($"- {t}");

Console.WriteLine("");
Console.WriteLine("Unmapped columns");
Console.WriteLine("================");
foreach(var g in detections.ColumnReferences.Where(x => !db.Model.Tables.SelectMany(t => t.Columns).Any(m => m.Table.Name == x.TableName && m.Name == x.column)).GroupBy(cr => cr.TableName))
{
    Console.WriteLine($"- {g.Key}");
    foreach (var col in g.Select(x => x.column).Distinct())
        Console.WriteLine($"    - {col}");
}

Console.WriteLine("");
Console.WriteLine("Unmapped measures");
Console.WriteLine("================");
foreach (var g in detections.MeasureReferences.Where(x => !db.Model.Tables.SelectMany(t => t.Measures).Any(m => m.Table.Name == x.TableName && m.Name == x.measure)).GroupBy(cr => cr.TableName))
{
    Console.WriteLine($"- {g.Key}");
    foreach (var col in g.Select(x => x.measure).Distinct())
        Console.WriteLine($"    - {col}");
}


Process.Start("explorer.exe", "\"" + outputArchivePath + "\"");

Console.ReadKey();

//var detections = new Detections();
//UnstuffTransform ut = new UnstuffTransform(new DummyLogger<UnstuffTransform>());
//DetectModelReferencesTransform detectRefs = new DetectModelReferencesTransform(detections);



//string pbix = @"C:\Dropbox (RwHealth)\WLT\Flow Tool - WLT\Modules\First Referrals, OA\First Referrals, OA (WLT).pbix";
//string folder = @"C:\TEst\proba2";
//string repackedPbix = @"C:\TEst\repacked.pbix";

//// 1. procitaj
//var report = PbiReportLoader.LoadFromPbiArchive(pbix);

////var dataModelDhcft = TabularModel.LoadFromSSAS("localhost\\rc2022", "Test123");
////dataModelDhcft.SaveToFolder("C:\\TEst\\DataModelComparison");

////TabularModel.LoadFromPbitFile(@"C:\Dropbox (RwHealth)\WLT\Flow Tool - WLT\Mental Health Flow Tool (WLT) - v1.0.20 - Development Master TEMPLATE.pbit")
////    .SetCompatibilityLimit(1500)
////    .PullUpExpressions()
////    .StripAutoDateTables()
////    .StripCultures()
////    .SaveToFolder("C:\\TEst\\DataModelComparison");

////// 2. spremi (originalno)
//var folderStore = new ReportFolderStore(folder);
//folderStore.EnableQueryMinification = false;
//folderStore.Save(report);

////// 2. spremi (minificirano)
//folderStore.EnableQueryMinification = true;
//folderStore.Save(report);

//var clone = JObject.Parse(report.Layout.ToString());
//ut.Transform(clone);
//detectRefs.Transform(clone);


////// 3. deminificiraj u folderu
//report = folderStore.Read();
//folderStore.EnableQueryMinification = false;
//folderStore.Save(report);

//// 4. procitaj tako deminificirano i zapakiraj
//report = folderStore.Read();
//var rfs2 = new PBIArchiveStore(repackedPbix);
//rfs2.Save(report);

//// 5. upali i testiraj
//Process.Start("explorer.exe", repackedPbix);
