// See https://aka.ms/new-console-template for more information
using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Packer2.Library;
using Packer2.Library.Report.Transforms;
using LibGit2Sharp;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter(null, LogLevel.Trace)
        .AddConsole();
});

// todo:
// 1. find unused database objects
//      - check dax (checking M not needed if tables can't reference each other but only expressions)
//      - check report references (infonav)
// 2. ability to rename measures without mentioning table name
// 3. abiltiy to move measure from table to table
// 4. ability to fix up measure parent table automatically (will fix all measures that have been moved)
// 5. find unused model extensions (measures defined inside the report)
// 6. when replacing table names in queries, also replace it in #config.modelExtensions area (e.g. "Advanced Analytics, Productivity, All Services (WLT).pbix")
// 7. include unmatched hierarchies/levels


var allDetections = new Detections();

var folder = @"C:\Dropbox (RwHealth)\WLT\Flow Tool - WLT\Modules";
var pbiFiles = Directory.GetFiles(folder, "*(WLT).pbix", SearchOption.AllDirectories);

pbiFiles = pbiFiles.Skip(4).Take(1).ToArray();

foreach (var originalPbix in pbiFiles)
{
    string name = Path.GetFileNameWithoutExtension(originalPbix);
    string workingPbix = Path.Combine(@"c:\test\wlt", name + ".pbix");

    // we might make manual changes to the pbix and we don't want to overwrite these by starting with the original again
    if (!File.Exists(workingPbix))
        File.Copy(originalPbix, workingPbix);

    var detections = new Detections();
    try
    {
        string repoFolder = Path.Combine(@"c:\test\wlt", name);

        // load the report from the folder if it's already been worked on manually
        // (indicated by the presence of the .git folder), from pbix if not.
        PowerBIReport report = LoadOrInitializeRepository(workingPbix, repoFolder);
        
        PerformReplacements(report);
        report
            .DetectModelExtensions(out var extensions)
            .DetectReferences(detections)
            .SaveToFolder(repoFolder)
            .SwitchToSSASDataSource("server=LTP-220118-ANA\\RC2022;database=test123")
            .SaveToPbiArchive(workingPbix);

        detections.Exclude(extensions);

        allDetections.Add(detections);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
    PrintDetections(detections);

    // Process.Start("explorer.exe", "\"" + outputArchivePath + "\"");

    //Console.Write("Processed file. Press enter to resume...");
    //Console.ReadLine();

    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("*********************************************");
    Console.WriteLine();
}

PowerBIReport LoadOrInitializeRepository(string pbix, string repoFolder)
{
    if (!Directory.Exists(Path.Combine(repoFolder, ".git")))
    {
        var sig = new Signature("antonio", "antonio@realworld.health", DateTimeOffset.Now);
        PbiReportLoader
            .LoadFromPbiArchive(pbix)
            .SaveToFolder(repoFolder);
        Repository.Init(repoFolder);
        using (var repo = new Repository(repoFolder))
        {
            Commands.Stage(repo, "*");
            repo.Commit("initial commit", sig, sig);
        }
    }

    Console.WriteLine($"Processing from folder: '{repoFolder}'");
    return PbiReportLoader.LoadFromFolder(repoFolder);
    // return PbiReportLoader.LoadFromPbiArchive(pbix);
}

void GitCommitChanges(string repoFolder, string v)
{
    throw new NotImplementedException();
}

void GitCreateRepository(string repoFolder)
{
    
}

Console.WriteLine("*********************************************");
Console.WriteLine("*********************************************");

PrintDetections(allDetections);

void PrintDetections(Detections detections)
{
    var uniqueTables = detections.ColumnReferences.Select(c => c.TableName).Union(detections.MeasureReferences.Select(m => m.TableName)).ToHashSet();
    var db = TabularModel.LoadFromSSAS(@"localhost\rc2022", "test123");
    var serverTables = db.Model.Tables.Select(t => t.Name).ToHashSet();

    var unmappedTables = uniqueTables.Except(serverTables).ToList();

    if (unmappedTables.Any())
    {
        Console.WriteLine("Unmapped tables:");
        Console.WriteLine("================");
        foreach (var t in unmappedTables)
            Console.WriteLine($"- {t}");
    }

    var unmappedCols = detections.ColumnReferences.Where(x => !db.Model.Tables.SelectMany(t => t.Columns).Any(m => m.Table.Name == x.TableName && m.Name == x.column)).GroupBy(cr => cr.TableName).ToList();
    if (unmappedCols.Any())
    {
        Console.WriteLine("");
        Console.WriteLine("Unmapped columns");
        Console.WriteLine("================");
        foreach (var g in unmappedCols)
        {
            Console.WriteLine($"- {g.Key}");
            foreach (var col in g.GroupBy(x => x.column))
                Console.WriteLine($"    - {col.Key} ({col.Count()})");
        }
    }

    var unmappedMeasures = detections.MeasureReferences.Where(x => !db.Model.Tables.SelectMany(t => t.Measures).Any(m => m.Table.Name == x.TableName && m.Name == x.measure)).GroupBy(cr => cr.TableName);
    if (unmappedMeasures.Any())
    {
        Console.WriteLine("");
        Console.WriteLine("Unmapped measures");
        Console.WriteLine("================");
        foreach (var g in unmappedMeasures)
        {
            Console.WriteLine($"- {g.Key}");
            foreach (var meas in g.GroupBy(x => x.measure))
                Console.WriteLine($"    - {meas.Key} ({meas.Count()})");
        }
    }
}


void PerformReplacements(PowerBIReport report)
{
    report.ReplaceTableReference("GPs", "GP Practices");
    report.ReplaceModelReference("GP Practices", "WLT PCN", "PCN_Group");

    report.ReplaceTableReference("Days of Week", "Weekdays");

    report.ReplaceTableReference("WardLeave", "Ward Leave");

    report.ReplaceTableReference("Localities", "Local Authorities");

    report.ReplaceTableReference("Latest Week", "Last Model Date");

    report.ReplaceTableReference("Acuity Tier", "Acuity Tier (T0)");
    report.ReplaceModelReference("Acuity Tier (T0)", "Service/Ward", "Service");
    report.ReplaceModelReference("Acuity Tier (T0)", "Service/Ward (Trial Balance LoD)", "Service (Trial Balance LoD)");
    report.ReplaceModelReference("Acuity Tier (T0)", "Simplified Service Type", "Service Type");

    report.ReplaceTableReference("Acuity Tier (Previous)", "Acuity Tier (T-1)");
    report.ReplaceTableReference("Acuity Tier (Previous Service)", "Acuity Tier (T-1)");
    report.ReplaceModelReference("Acuity Tier (T-1)", "Service/Ward", "Service");
    report.ReplaceModelReference("Acuity Tier (T-1)", "Simplified Service Type", "Service Type");

    report.ReplaceTableReference("Acuity Tier (T-2 Service)", "Acuity Tier (T-2)");
    report.ReplaceModelReference("Acuity Tier (T-2)", "Service/Ward", "Service");
    report.ReplaceModelReference("Acuity Tier (T-2)", "Simplified Service Type", "Service Type");

    report.ReplaceTableReference("Acuity Tier (T+1 Service)", "Acuity Tier (T+1)");
    report.ReplaceModelReference("Acuity Tier (T+1)", "Service/Ward", "Service");
    report.ReplaceModelReference("Acuity Tier (T+1)", "Simplified Service Type", "Service Type");

    report.ReplaceTableReference("Acuity Tier (T+2 Service)", "Acuity Tier (T+2)");
    report.ReplaceModelReference("Acuity Tier (T+2)", "Service/Ward", "Service");
    report.ReplaceModelReference("Acuity Tier (T+2)", "Simplified Service Type", "Service Type");

    report.ReplaceTableReference("Acuity Tier (Next Service Within 6 months)", "Acuity Tier (T+1_w6M)");
    report.ReplaceModelReference("Acuity Tier (T+1_w6M)", "Simplified Service Type", "Service Type");

    report.ReplaceTableReference("Care Changes", "Flows");
    report.ReplaceTableReference("Care Changes data", "Flows");
    report.ReplaceModelReference("Flows", "Service Flows for graphing", "m_Flows");
    report.ReplaceModelReference("Flows", "Next Service within 6 months", "T+1_w6M Service");
    report.ReplaceModelReference("Flows", "Resultant Service", "T0 Service");
    report.ReplaceModelReference("Flows", "m_ServiceFlows_NoCalendarTableRelationship", "m_Flows_NoCalendarTableRelationship");
    // todo: remove my added m_Flows_InPeriod measure => we're using m_Flows instead

    report.ReplaceModelReference("Crisis", "m_CountCrisisPresentation", "m_CrisisPresentations_Count");

    report.ReplaceModelReference("Local Authorities", "Local Authority Name", "Local Authority");

    report.ReplaceModelReference("Referral Spells", "m_CountReferralsSpells", "m_ReferralSpellsOpening_Count");
    report.ReplaceModelReference("Referral Spells", "m_ReferralSpells_AverageCaseLength_Days", "m_ReferralSpells_AverageLength_Days");

    report.ReplaceModelReference("Trial Balance", "m_WTEs", "m_WTEs_AllPaid");
    report.ReplaceModelReference("Trial Balance", "m_Cost_Pay_PerWTE_InPeriod", "m_Cost_PerWTE_AllPaid_InPeriod");
    report.ReplaceModelReference("Trial Balance", "m_Cost_Pay_PerContact", "m_Cost_PerContact");
    report.ReplaceModelReference("Trial Balance", "m_Cost_Pay_PerCase", "m_CostPerReferral");
    report.ReplaceModelReference("Trial Balance", "m_Cost_Pay_1000s", "m_Cost_1000s");
    report.ReplaceModelReference("Trial Balance", "Cost Centre Desc", "Cost Centre Code Desc");

    report.ReplaceTableReference("flow ReferralSourcesSimplified", "Pathways");
    report.ReplaceModelReference("Pathways", "Referral Source Simplified", "Pathway Referral source");

    report.ReplaceModelReference("Localities", "Locality", "Local Authority");

    report.ReplaceModelReference("Last Model Date", "Model Update Date", "m_LastUpdated_VisualLabel");

    report.ReplaceTableReference("Calendar Helper", "Calendar");
    report.ReplaceModelReference("Calendar", "Is Current 3 Years", "Is Last 3 Years");
    report.ReplaceModelReference("Calendar", "m_DatesSelected", "DateRangeHeader");
    report.ReplaceModelReference("Calendar", "Is Current 52 Weeks", "Is Last 52 Weeks");
    report.ReplaceModelReference("Calendar", "Is Current 6 Months", "Is Last 6 Months");
    report.ReplaceModelReference("Calendar", "Is Current 3 Months", "Is Last 3 Months");
    report.ReplaceModelReference("Calendar", "Is Current 57 Weeks", "Is Last 57 Weeks");
    report.ReplaceModelReference("Calendar", "Is Current 30 Days", "Is Last 30 Days");
    report.ReplaceModelReference("Calendar", "Is Current Week", "Is Last Week");

    report.ReplaceTableReference("Referrals and Ward Stays", "Care Episodes");
    // report.ReplaceModelReference("Calendar", "Completed Month", "???");

    report.ReplaceTableReference("Staff Types", "Subjective Code Descriptions");
    report.ReplaceModelReference("Subjective Code Descriptions", "Staff Grouping", "Staff Type");

    report.ReplaceModelReference("Patients", "Ethnicity/BAME Group", "Ethnic Category (group)");
    report.ReplaceModelReference("EthnicityDescription", "Ethnicity/BAME Group", "Ethnic Category");

    report.ReplaceModelReference("Admissions", "Adult Acute & PICU Admissions in last Year", "Acute or PICU Admissions in last Year");
    report.ReplaceModelReference("Admissions", "Admission Ward", "T0 Service");
    report.ReplaceModelReference("Admissions", "LA on Admission", "Local Authority on Admission");

    report.ReplaceTableReference("Acuity Tier (CATT)", "Acuity Tier HTT");
    report.ReplaceModelReference("Acuity Tier HTT", "Service/Ward", "Service");

    report.RebaseMeasure("m_Caseload_Total_EndofPeriod_Stratified_byIntensity", "Referral Spells");
    report.RebaseMeasure("m_Caseload_Total_EndofPeriod_Stratified_byLastContact", "Referral Spells");
}

Console.ReadKey();
