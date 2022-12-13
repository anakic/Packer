// See https://aka.ms/new-console-template for more information
using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Packer2.Library;
using Packer2.Library.Report.Transforms;
using LibGit2Sharp;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Microsoft.AnalysisServices.Tabular;
using System.Diagnostics;

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
// + 3. abiltiy to move measure from table to table
// 4. ability to fix up measure parent table automatically (will fix all measures that have been moved)
// 5. find unused model extensions (measures defined inside the report)
// 6. when replacing table names in queries, also replace it in #config.modelExtensions area (e.g. "Advanced Analytics, Productivity, All Services (WLT).pbix")
// 7. include unmatched hierarchies/levels

bool skipPrompts = false;
Mappings renames = CreateRenames();

var folder = @"C:\Dropbox (RwHealth)\WLT\Flow Tool - WLT\Modules";
var pbiFiles = Directory.GetFiles(folder, "*(WLT).pbix", SearchOption.AllDirectories);

var db = TabularModel.LoadFromSSAS(@"localhost\rc2022", "test234");
var srv = new Server();
srv.Connect(@"Data source=powerbi://api.powerbi.com/v1.0/myorg/WLT | PROD;initial catalog=Mental Health Flow Tool (WLT)");
var oldDb = srv.Databases[0];

var allDetections = new Detections();

int i = 1;
foreach (var originalPbix in pbiFiles)
{
    string name = Path.GetFileNameWithoutExtension(originalPbix);
    string workingPbix = Path.Combine(@"c:\test\wlt", name + ".pbix");

    // we might make manual changes to the pbix and we don't want to overwrite these by starting with the original again
    if (!File.Exists(workingPbix))
        File.Copy(originalPbix, workingPbix);

    Console.Write($"{i}: ");
    try
    {
        bool redo = false;
        do
        {
            var detections = new Detections();
            string repoFolder = Path.Combine(@"c:\test\wlt", name);

            // load the report from the folder if it's already been worked on manually
            // (indicated by the presence of the .git folder), from pbix if not.
            PowerBIReport report = LoadOrInitializeRepository(workingPbix, repoFolder);

            report.ReplaceModelReferences(renames);
            report
                .DetectModelExtensions(out var extensions)
                .DetectReferences(detections)
                .SaveToFolder(repoFolder)
                .SwitchToSSASDataSource("server=LTP-220118-ANA\\RC2022;database=test123")
                .SaveToPbiArchive(workingPbix);

            detections.Exclude(extensions);

            allDetections.Add(detections);

            PrintDetections(detections, db, oldDb);
            Console.WriteLine("");
            redo = InteractiveLoop(workingPbix, repoFolder);
        }
        while (redo);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }

    Console.WriteLine();
    Console.WriteLine("*************************************************************************************");
    Console.WriteLine();
    i++;
}

Console.WriteLine("*************************************************************************************");
Console.WriteLine("*************************************************************************************");
Console.WriteLine("*************************************************************************************");

PrintDetections(allDetections, db, oldDb);

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Processing complete");
Console.ResetColor();


PowerBIReport LoadOrInitializeRepository(string pbix, string repoFolder)
{
    if (!Directory.Exists(Path.Combine(repoFolder, ".git")))
    {
        PbiReportLoader
            .LoadFromPbiArchive(pbix)
            .SaveToFolder(repoFolder);
        Repository.Init(repoFolder);
        Commit(repoFolder, "initial commit");
    }

    Console.WriteLine($"Processing from folder: '{repoFolder}'");
    return PbiReportLoader.LoadFromFolder(repoFolder);
}


void PrintDetections(Detections detections, Database ssasModel, Database oldModel)
{
    var uniqueTables = detections.ColumnReferences.Select(c => c.TableName).Union(detections.MeasureReferences.Select(m => m.TableName)).ToHashSet();
    var serverTables = db.Model.Tables.Select(t => t.Name).ToHashSet();

    var unmappedTables = uniqueTables.Except(serverTables).ToList();

    if (unmappedTables.Any())
    {
        Console.WriteLine("Unmapped tables:");
        Console.WriteLine("================");
        foreach (var t in unmappedTables)
            Console.WriteLine($"- {t}");
    }

    var unmappedCols = detections.ColumnReferences.Where(x => !IsColumnInModel(new[] { x.TableName }, x.column, db)).GroupBy(cr => cr.TableName).ToList();
    if (unmappedCols.Any())
    {
        Console.WriteLine("");
        Console.WriteLine("Unmapped columns");
        Console.WriteLine("================");
        foreach (var g in unmappedCols)
        {
            Console.WriteLine($"- {g.Key}");
            foreach (var col in g.GroupBy(x => x.column))
            {
                bool isInModel = IsColumnInModel(renames.GetTablesMappedTo(g.Key).Append(g.Key).Distinct(), col.Key, oldDb);
                string prefix = isInModel ? "-" : "X";
                if (!isInModel)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"    {prefix} {col.Key} ({col.Count()})");
                Console.ResetColor();
            }
        }
    }

    var unmappedMeasures = detections.MeasureReferences.Where(x => !IsMeasureInModel(x.measure, db)).GroupBy(cr => cr.TableName);
    if (unmappedMeasures.Any())
    {
        Console.WriteLine("");
        Console.WriteLine("Unmapped measures");
        Console.WriteLine("================");
        foreach (var g in unmappedMeasures)
        {
            Console.WriteLine($"- {g.Key}");
            foreach (var meas in g.GroupBy(x => x.measure))
            {
                bool isInModel = IsMeasureInModel(meas.Key, oldDb);
                string prefix = isInModel ? "-" : "X";
                if (!isInModel)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"    {prefix} {meas.Key} ({meas.Count()})");
                Console.ResetColor();
            }
        }
    }
}

bool InteractiveLoop(string archivePath, string folderPath)
{
    if (skipPrompts)
        return false;

    bool done = false;
    do
    {
        Console.WriteLine("C - continue, P - Open in PowerBI, V - Open in VSCode, F - Extract to folder, A - Assemble archive, G - Create Git commit, S - Skip all prompts, R - Redo analysis, Q - Quit");
        var answer = Console.ReadKey();
        Console.WriteLine();
        if (answer.Key == ConsoleKey.Q)
            Environment.Exit(0);
        else if (answer.Key == ConsoleKey.R)
            return true;
        else if (answer.Key == ConsoleKey.S)
        {
            skipPrompts = true;
            done = true;
        }
        else if (answer.Key == ConsoleKey.C)
        {
            done = true;
        }
        else if (answer.Key == ConsoleKey.P)
        {
            Process.Start("explorer.exe", "\"" + archivePath + "\"");
        }
        else if (answer.Key == ConsoleKey.F)
        {
            PbiReportLoader.LoadFromPbiArchive(archivePath).SaveToFolder(folderPath);
        }
        else if (answer.Key == ConsoleKey.A)
        {
            PbiReportLoader.LoadFromFolder(folderPath).SaveToPbiArchive(archivePath);
        }
        else if (answer.Key == ConsoleKey.G)
        {
            Console.WriteLine("Enter commit message: ");
            var message = Console.ReadLine()!;
            Commit(folderPath, message);
        }
        else if (answer.Key == ConsoleKey.V)
        {
            Process.Start("C:\\Users\\AntonioNakic-Alfirev\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe", "\"" + folderPath + "\"");
        }
    }
    while (!done);

    return false;
}

bool IsColumnInModel(IEnumerable<string> tableNames, string columnName, Database db)
    => db.Model.Tables.SelectMany(t => t.Columns).Any(m => tableNames.Contains(m.Table.Name) && m.Name == columnName);

bool IsMeasureInModel(string measureName, Database db)
    => db.Model.Tables.SelectMany(t => t.Measures).Any(m => /*m.Table.Name == tableName && */m.Name == measureName);

Mappings CreateRenames()
{
    var renames = new Mappings();

    renames.AddTableRename("GPs", "GP Practices");
    renames.AddRename("GP Practices", "WLT PCN", "PCN_Group");

    renames.AddTableRename("Days of Week", "Weekdays");

    renames.AddMove("Weekdays", "Weekday or Weekend", "Calendar", "Weekday or Weekend");

    renames.AddTableRename("WardLeave", "Ward Leave");

    renames.AddTableRename("Localities", "Local Authorities");

    renames.AddTableRename("Latest Week", "Last Model Date");

    renames.AddTableRename("Acuity Tier", "Acuity Tier (T0)");
    renames.AddRename("Acuity Tier (T0)", "Service/Ward", "Service");
    renames.AddRename("Acuity Tier (T0)", "Service/Ward (Trial Balance LoD)", "Service (Trial Balance LoD)");
    renames.AddRename("Acuity Tier (T0)", "Simplified Service Type", "Service Type");

    renames.AddTableRename("Acuity Tier (Previous)", "Acuity Tier (T-1)");
    renames.AddTableRename("Acuity Tier (Previous Service)", "Acuity Tier (T-1)");
    renames.AddRename("Acuity Tier (T-1)", "Service/Ward", "Service");
    renames.AddRename("Acuity Tier (T-1)", "Simplified Service Type", "Service Type");

    renames.AddTableRename("Acuity Tier (T-2 Service)", "Acuity Tier (T-2)");
    renames.AddRename("Acuity Tier (T-2)", "Service/Ward", "Service");
    renames.AddRename("Acuity Tier (T-2)", "Simplified Service Type", "Service Type");

    renames.AddTableRename("Acuity Tier (T+1 Service)", "Acuity Tier (T+1)");
    renames.AddRename("Acuity Tier (T+1)", "Service/Ward", "Service");
    renames.AddRename("Acuity Tier (T+1)", "Simplified Service Type", "Service Type");
    renames.AddRename("Acuity Tier (T+1)", "Conversion into Next Service_Latest 3 months", "m_%ConversionToT+1_Latest3Months");
    renames.AddRename("Acuity Tier (T+1)", "Conversion into Next Service_excl Latest 3 months", "m_%ConversionToT+1_NotLatest3Months ");

    renames.AddTableRename("Acuity Tier (T+2 Service)", "Acuity Tier (T+2)");
    renames.AddRename("Acuity Tier (T+2)", "Service/Ward", "Service");
    renames.AddRename("Acuity Tier (T+2)", "Simplified Service Type", "Service Type");

    renames.AddTableRename("Acuity Tier (Next Service Within 6 months)", "Acuity Tier (T+1_w6M)");
    renames.AddRename("Acuity Tier (T+1_w6M)", "Simplified Service Type", "Service Type");

    renames.AddTableRename("Care Changes", "Flows");
    renames.AddTableRename("Care Changes data", "Flows");
    renames.AddRename("Flows", "Service Flows for graphing", "m_Flows");
    renames.AddRename("Flows", "Next Service within 6 months", "T+1_w6M Service");
    renames.AddRename("Flows", "Resultant Service", "T0 Service");
    renames.AddRename("Flows", "m_ServiceFlows_NoCalendarTableRelationship", "m_Flows_NoCalendarTableRelationship");
    renames.AddRename("Flows", "Service Flows", "m_Flows_0");
    renames.AddRename("Flows", "Previous Service", "T-1 Service");
    renames.AddRename("Flows", "30-day Readmission Rate", "m_ReadmissionRate_30days");
    renames.AddRename("Flows", "Conversion Rate to Inpatient", "m_ConversionRate_Inpatient");

    renames.AddRename("Flows", "m_ReadmissionRate_30Days", "m_ReadmissionRate_30days");
    renames.AddRename("Flows", "m_ConversionRate_ToInpatient", "m_ConversionRate_Inpatient");

    renames.AddMove("Gender (Simplifcations)", "Gender (Simplification)", "Patients", "Gender");
    renames.AddMove("Gender", "Gender (Simplification)", "Patients", "Gender");

    renames.AddRename("Flows", "Average Flows", "m_Flows_AverageperWeek");
    // todo: remove my added m_Flows_InPeriod measure => we're using m_Flows instead

    renames.AddRename("Crisis", "m_CountCrisisPresentation", "m_CrisisPresentations_Count");

    renames.AddRename("Local Authorities", "Local Authority Name", "Local Authority");

    renames.AddRename("Referral Spells", "m_CountReferralsSpells", "m_ReferralSpellsOpening_Count");
    renames.AddRename("Referral Spells", "m_ReferralSpells_AverageCaseLength_Days", "m_ReferralSpells_AverageLength_Days");

    renames.AddRename("Trial Balance", "m_WTEs", "m_WTEs_AllPaid");
    renames.AddRename("Trial Balance", "m_Cost_Pay_PerWTE_InPeriod", "m_Cost_PerWTE_AllPaid_InPeriod");
    renames.AddRename("Trial Balance", "m_Cost_Pay_PerContact", "m_Cost_PerContact");
    renames.AddRename("Trial Balance", "m_Cost_Pay_PerCase", "m_CostPerReferral");
    renames.AddRename("Trial Balance", "m_Cost_Pay_1000s", "m_Cost_1000s");
    renames.AddRename("Trial Balance", "Cost Centre Desc", "Cost Centre Code Desc");

    renames.AddTableRename("flow ReferralSourcesSimplified", "Pathways");
    renames.AddRename("Pathways", "Referral Source Simplified", "Pathway Referral source");

    renames.AddRename("Localities", "Locality", "Local Authority");

    renames.AddRename("Last Model Date", "Model Update Date", "m_LastUpdated_VisualLabel");

    renames.AddTableRename("Calendar Helper", "Calendar");
    renames.AddRename("Calendar", "Is Current 3 Years", "Is Last 3 Years");
    renames.AddRename("Calendar", "m_DatesSelected", "DateRangeHeader");
    renames.AddRename("Calendar", "Is Current 52 Weeks", "Is Last 52 Weeks");
    renames.AddRename("Calendar", "Is Current 6 Months", "Is Last 6 Months");
    renames.AddRename("Calendar", "Is Current 3 Months", "Is Last 3 Months");
    renames.AddRename("Calendar", "Is Current 57 Weeks", "Is Last 57 Weeks");
    renames.AddRename("Calendar", "Is Current 30 Days", "Is Last 30 Days");
    renames.AddRename("Calendar", "Is Current Week", "Is Last Week");

    renames.AddTableRename("Referrals and Ward Stays", "Care Episodes");
    // renames.AddRename("Calendar", "Completed Month", "???");

    renames.AddTableRename("Staff Types", "Subjective Code Descriptions");
    renames.AddRename("Subjective Code Descriptions", "Staff Grouping", "Staff Type");

    renames.AddRename("Patients", "Ethnicity/BAME Group", "Ethnic Category (group)");
    renames.AddRename("EthnicityDescription", "Ethnicity/BAME Group", "Ethnic Category");

    renames.AddRename("Admissions", "Adult Acute & PICU Admissions in last Year", "Acute or PICU Admissions in last Year");
    renames.AddRename("Admissions", "Admission Ward", "T0 Service");
    renames.AddRename("Admissions", "LA on Admission", "Local Authority on Admission");
    renames.AddRename("Admissions", "Admission Gateway Simplified", "T-1 Service Type");
    renames.AddRename("Admissions", "Age at Admission (group)", "Age at Admission (band)");
    renames.AddRename("Admissions", "Admitted Under Section (all)", "MHA Section Admission Status");

    renames.AddTableRename("Acuity Tier (CATT)", "Acuity Tier HTT");
    renames.AddRename("Acuity Tier HTT", "Service/Ward", "Service");

    renames.AddMeasureRebase("m_Caseload_Total_EndofPeriod_Stratified_byIntensity", "Referral Spells");
    renames.AddMeasureRebase("m_Caseload_Total_EndofPeriod_Stratified_byLastContact", "Referral Spells");

    renames.AddTableRename("Days from Trust Entry to Admission", "Days from Trust Entry to Admission Label Rank");
    renames.AddRename("Days from Trust Entry to Admission Label Rank", "Days From Trust Entry to Admission", "Days from Trust entry to Admission (bins)");

    return renames;
}

Console.ReadKey();

static void Commit(string repoFolder, string message)
{
    var sig = new Signature("antonio", "antonio@realworld.health", DateTimeOffset.Now);
    using (var repo = new Repository(repoFolder))
    {
        Commands.Stage(repo, "*");
        repo.Commit(message, sig, sig);
    }
}