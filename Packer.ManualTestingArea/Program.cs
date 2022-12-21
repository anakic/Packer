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

bool skipPrompts = false;
Mappings renames = CreateRenames();

var folder = @"C:\Dropbox (RwHealth)\WLT\Flow Tool - WLT\Modules";
var pbiFiles = Directory.GetFiles(folder, "*(WLT).pbix", SearchOption.AllDirectories)
    .Select(x => x.ToLower())
    .Where(f => !f.Contains("\\old\\"))
    .Where(f => !f.Contains("archive\\"))
    .Where(f => !f.Contains("benchmarking"))
    .Where(f => !f.Contains("explainers"))
    .Where(f => !f.Contains("crises, camhs (wlt)"))
    .Where(f => !f.Contains("exec flow headlines, awa"));

var ssasConnStr = "server=LTP-220118-ANA\\RC2022;database=test234";
var db = TabularModel.LoadFromSSAS(ssasConnStr);
var srv = new Server();
srv.Connect(@"Data source=powerbi://api.powerbi.com/v1.0/myorg/WLT | PROD;initial catalog=Mental Health Flow Tool (WLT)");
var oldDb = srv.Databases[0];

var allDetections = new Detections();

int i = 1;
foreach (var originalPbix in pbiFiles)
{
    string name = Path.GetFileNameWithoutExtension(originalPbix);
    string repoFolder = Path.Combine(@"C:\work\DHCFT\Modules\WLT", name);
    string workingPbix = repoFolder + ".pbix";

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

            // load the report from the folder if it's already been worked on manually
            // (indicated by the presence of the .git folder), from pbix if not.
            PowerBIReport report = LoadOrInitializeRepository(workingPbix, repoFolder);

            report.ReplaceModelReferences(renames);
            report
                .DetectModelExtensions(out var extensions)
                .DetectReferences(detections)
                .SaveToFolder(repoFolder)
                .SwitchToSSASDataSource(ssasConnStr)
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
    if (!Directory.Exists(repoFolder))
    {
        PbiReportLoader
            .LoadFromPbiArchive(pbix)
            .SaveToFolder(repoFolder);
        //Repository.Init(repoFolder);
        //Commit(repoFolder, "initial commit");
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

    var unmappedCols = detections.ColumnReferences.Where(x => !IsColumnInModel(new[] { x.TableName }, x.Column, db)).GroupBy(cr => cr.TableName).ToList();
    if (unmappedCols.Any())
    {
        Console.WriteLine("");
        Console.WriteLine("Unmapped columns");
        Console.WriteLine("================");
        foreach (var g in unmappedCols)
        {
            Console.WriteLine($"- {g.Key}");
            foreach (var col in g.GroupBy(x => x.Column))
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

    var unmappedMeasures = detections.MeasureReferences.Where(x => !IsMeasureInModel(new[] { x.TableName }, x.Measure, db)).GroupBy(cr => cr.TableName);
    if (unmappedMeasures.Any())
    {
        Console.WriteLine("");
        Console.WriteLine("Unmapped measures");
        Console.WriteLine("================");
        foreach (var g in unmappedMeasures)
        {
            Console.WriteLine($"- {g.Key}");
            foreach (var meas in g.GroupBy(x => x.Measure))
            {
                bool isInModel = IsMeasureInModel(renames.GetTablesMappedTo(g.Key).Append(g.Key).Distinct(), meas.Key, oldDb);
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
            PbiReportLoader.LoadFromFolder(folderPath)
                .SwitchToSSASDataSource(ssasConnStr)
                .SaveToPbiArchive(archivePath);
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

bool IsMeasureInModel(IEnumerable<string> tableNames, string measureName, Database db)
    => db.Model.Tables.SelectMany(t => t.Measures).Any(m => tableNames.Contains(m.Table.Name) && m.Name == measureName);

Mappings CreateRenames()
{
    var mappings = new Mappings();

    mappings.Table("GPs")
        .MapTo("GP Practices")
        .MapObjectTo("WLT PCN", "PCN_Group", "PCN");

    mappings.Table("PCNs").MapTo("PCN");

    mappings.Table("GP Practices")
        .MapObjectTo("PCN_Group", "PCN_Group", "PCN");

    mappings.Table("WardLeave")
        .MapTo("Ward Leave");

    mappings.Table("Latest Week")
        .MapTo("Last Model Date")
        .MapObjectTo("Model Update Date", "m_LastUpdated_VisualLabel");

    mappings.Table("Acuity Tier").MapTo("Acuity Tier (T0)");
    mappings.Table("Acuity Tier (T0)")
        .MapObjectTo("Service/Ward", "Service")
        .MapObjectTo("Simplified Service Type", "Service Type")
        .MapObjectTo("Service/Ward (Trial Balance LoD)", "Service (Trial Balance LoD)")
        .MapObjectTo("m_serviceflows_inflows_HTT", "m_Flows_Inflows_HTT")
        .MapObjectTo("m_serviceflows_outflows_HTT", "m_Flows_Outflows_HTT")
        .MapObjectTo("Average Flows", "m_Flows_AverageperWeek", "Flows")
        .MapObjectTo("m_Caseload_Total_EndofPeriod_Stratified_byIntensity", "m_Caseload_Total_EndofPeriod_Stratified_byIntensity", "Referral Spells")
        .MapObjectTo("m_Caseload_Total_EndofPeriod_Stratified_byLastContact", "m_Caseload_Total_EndofPeriod_Stratified_byLastContact", "Referral Spells")
        .MapObjectTo("m_%Change_YearonYear_DirectCaseload", "m_%Change_YearonYear_DirectCaseload", "Flows")
        .MapObjectTo("m_Flows_Inflows_HTT", "m_Flows_Inflows_HTT", "Flows");

    mappings.Table("Acuity Tier (Previous)").MapTo("Acuity Tier (T-1)");
    mappings.Table("Acuity Tier (Previous Service)").MapTo("Acuity Tier (T-1)");
    mappings.Table("Acuity Tier (T-1)")
        .MapObjectTo("Service/Ward", "Service")
        .MapObjectTo("Simplified Service Type", "Service Type");

    mappings.Table("Acuity Tier (T-2 Service)").MapTo("Acuity Tier (T-2)");
    mappings.Table("Acuity Tier (T-2)")
        .MapTo("Acuity Tier (T-2)")
        .MapObjectTo("Service/Ward", "Service")
        .MapObjectTo("Simplified Service Type", "Service Type");

    mappings.Table("Acuity Tier (T+1 Service)")
        .MapTo("Acuity Tier (T+1)");
    mappings.Table("Acuity Tier (T+1)")
        .MapObjectTo("Service/Ward", "Service")
        .MapObjectTo("Simplified Service Type", "Service Type")
        .MapObjectTo("Conversion into Next Service_Latest 3 months", "m_%ConversionToT+1_Latest3Months")
        .MapObjectTo("Conversion into Next Service_excl Latest 3 months", "m_%ConversionToT+1_NotLatest3Months");

    mappings.Table("Acuity Tier (T+2 Service)")
        .MapTo("Acuity Tier (T+2)");
    mappings.Table("Acuity Tier (T+2)")
        .MapTo("Acuity Tier (T+2)")
        .MapObjectTo("Service/Ward", "Service")
        .MapObjectTo("Simplified Service Type", "Service Type");

    mappings.Table("Acuity Tier (Next Service Within 6 months)")
        .MapTo("Acuity Tier (T+1_w6M)");
    mappings.Table("Acuity Tier (T+1_w6M)")
        .MapObjectTo("Simplified Service Type", "Service Type");

    mappings.Table("Care Changes").MapTo("Flows");
    mappings.Table("Care Changes data").MapTo("Flows");
    mappings.Table("Flows")
        .MapObjectTo("Service Flows for graphing", "m_Flows")
        .MapObjectTo("Next Service within 6 months", "T+1_w6M Service")
        .MapObjectTo("Resultant Service", "T0 Service")
        .MapObjectTo("Service Flows", "m_Flows_0")
        .MapObjectTo("Previous Service", "T-1 Service")
        .MapObjectTo("Average Flows", "m_Flows_AverageperWeek")
        .MapObjectTo("Last Known Gender", "Gender", "Patients")
        .MapObjectTo("Gender", "Gender", "Patients")
        .MapObjectTo("Month on Month Flow Change", "m_%Change_MonthonMonth_Flows")
        .MapObjectTo("MHA Section", "Last MHA Section at Flow")
        .MapObjectTo("Responsible Borough (Inferred)", "Responsible Area", "Responsible Area")
        .MapObjectTo("30-day Readmission Rate", "m_ReadmissionRate_30days")
        .MapObjectTo("Conversion Rate to Inpatient", "m_ConversionRate_Inpatient", "Admissions")
        .MapObjectTo("m_ConversionRate_ToInpatient", "m_ConversionRate_Inpatient", "Admissions")
        .MapObjectTo("m_ConversionRate_Inpatient", "m_ConversionRate_Inpatient", "Admissions")
        .MapObjectTo("m_ReadmissionRate_30Days", "m_ReadmissionRate_30days")
        .MapObjectTo("m_ServiceFlows_NoCalendarTableRelationship", "m_Flows_NoCalendarTableRelationship")
        .MapObjectTo("m_SPC_Measure", "m_Selected_Measure")
        .MapObjectTo("m_Caseload_Direct_CATT", "m_Caseload_Direct_HTT")
        // this measure was broken even before 
        //.MapObjectTo("Into HTT Flow", "m_Flows_Inflows_HTT")
        .MapObjectTo("service flows % change", "m_Flows_%change")
        .MapObjectTo("m_Compound_Weekly_Caseload_Rate", "m_CririsPresentations_CompoundWeeklyGrowthRate", "Crisis")
        .MapObjectTo("Net Adm/Dis", "Net Adm/Dis", "Acuity Tier (T0)");

    mappings.Table("Gender (Simplifcations)")
        .MapTo("Patients")
        .MapObjectTo("Gender (Simplification)", "Gender");

    mappings.Table("Gender")
        .MapTo("Patients")
        .MapObjectTo("Gender (Simplification)", "Gender");

    mappings.Table("Ethnicity")
        .MapTo("Patients");

    mappings.Table("Patients")
        .MapObjectTo("Gender (Simplification)", "Gender")
        .MapObjectTo("Ethnicity Simplification", "Ethnic Category (group)")
        .MapObjectTo("EthnicityDescription", "Ethnic Category (group)")
        .MapObjectTo("Ethnicity/BAME Group", "Ethnic Category");

    mappings.Table("Crisis")
        .MapObjectTo("m_CountCrisisPresentation", "m_CrisisPresentations_Count");

    mappings.Table("Local Authorities")
        .MapObjectTo("Local Authority Name", "Local Authority");

    mappings.Table("Referral Spells")
        .MapObjectTo("m_CountReferralsSpells", "m_ReferralSpellsOpening_Count")
        .MapObjectTo("m_ReferralSpells_AverageCaseLength_Days", "m_ReferralSpells_AverageLength_Days");

    mappings.Table("Trial Balance")
        .MapObjectTo("m_WTEs", "m_WTEs_AllPaid")
        .MapObjectTo("m_Cost_Pay_PerWTE_InPeriod", "m_Cost_PerWTE_AllPaid_InPeriod")
        .MapObjectTo("m_Cost_Pay_PerContact", "m_Cost_PerContact")
        .MapObjectTo("m_Cost_Pay_PerCase", "m_CostPerReferral")
        .MapObjectTo("m_Cost_Pay_1000s", "m_Cost_1000s")
        .MapObjectTo("Cost Centre Desc", "Cost Centre Code Desc");

    mappings.Table("flow ReferralSourcesSimplified")
        .MapTo("Pathways")
        .MapObjectTo("Referral Source Simplified", "Pathway Referral source");

    mappings.Table("Days of Week").MapTo("Calendar");
    mappings.Table("Weekdays").MapTo("Calendar");
    // mappings.Table("Calendar Helper").MapTo("Calendar");

    mappings.Table("Calendar")
        .MapObjectTo("Is Current 3 Years", "Is Last 3 Years")
        .MapObjectTo("Is Current 52 Weeks", "Is Last 52 Weeks")
        .MapObjectTo("Is Current 6 Months", "Is Last 6 Months")
        .MapObjectTo("Is Current 3 Months", "Is Last 3 Months")
        .MapObjectTo("Is Current 57 Weeks", "Is Last 57 Weeks")
        .MapObjectTo("Is Current 30 Days", "Is Last 30 Days")
        .MapObjectTo("Day", "Date")
        .MapObjectTo("Is Current Week", "Is Last Week");

    mappings.Table("Calendar")
        .MapObjectTo("m_DatesSelected", "DateRangeHeader");

    mappings.Table("Referrals and Ward Stays")
        .MapTo("Care Episodes");

    mappings.Table("Staff Types")
        .MapTo("Subjective Code Descriptions")
        .MapObjectTo("Staff Grouping", "Staff Type");

    //mappings.Table("EthnicityDescription")
    //    .MapObjectTo("Ethnicity/BAME Group", "Ethnic Category");

    mappings.Table("Admissions")
        .MapObjectTo("Adult Acute & PICU Admissions in last Year", "Acute or PICU Admissions in last Year")
        .MapObjectTo("Admission Ward", "T0 Service")
        .MapObjectTo("LA on Admission", "Local Authority on Admission")
        .MapObjectTo("Admission Gateway Simplified", "T-1 Service Type")
        .MapObjectTo("Age at Admission (group)", "Age at Admission (band)")
        .MapObjectTo("Resultant Service", "T0 Service")
        .MapObjectTo("MHA Section", "MHA Section Open on Admission")
        .MapObjectTo("Section Admission Label", "MHA Section Admission Status")
        .MapObjectTo("Admitted Under Section (all)", "MHA Section Admission Status")
        .MapObjectTo("Net Adm/Dis2", "m_NetAdmissions")
        .MapObjectTo("m_Discharges", "m_Discharges_NetAdmissions")
        .MapObjectTo("m_Discharges_NetAdmissions", "m_Discharges_NetAdmissions", "Acuity Tier (T0)")
        .MapObjectTo("m_NetAdmissions", "m_NetAdmissions", "Acuity Tier (T0)");

    mappings.Table("Crisis")
        .MapObjectTo("In-Hours or Out-of-Hours", "In or Out of Hours")
        .MapObjectTo("Assumed Responsible Borough at Crisis", "Responsible Area");

    mappings.Table("MHA Status (Simplified)")
        .MapObjectTo("MHA Status (Simplified)", "MHA Section Admission Status", "Admissions");

    mappings.Table("Section Admission Label")
        .MapObjectTo("Section Admission Label", "MHA Section Admission Status", "Admissions");

    mappings.Table("Acuity Tier (CATT)")
        .MapTo("Acuity Tier HTT")
        .MapObjectTo("Service/Ward", "Service");

    mappings.Table("Days from Trust Entry to Admission")
        .MapTo("Days from Trust Entry to Admission Label Rank");

    mappings.Table("Days from Trust Entry to Admission Label Rank")
        .MapObjectTo("Days From Trust Entry to Admission", "Days from Trust entry to Admission (bins)");

    mappings.Table("Responsible Boroughs (Inferred)").MapTo("Responsible Area");
    mappings.Table("Responsible Area")
        .MapObjectTo("Responsible Borough (Inferred)", "Responsible Area")
        .MapObjectTo("Longhand Responsible Borough (Inferred)", "Responsible Area_Longhand")
        .MapObjectTo("Shorthand Responsible Borough (Inferred)", "Responsible Area_Shorthand")
        .MapObjectTo("Locational Patch", "Responsible Area_Group");

    mappings.Table("Section Simp")
        .MapObjectTo("Admitted Under Section", "MHA Section Admission Status", "Admissions");

    return mappings;
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


//var test = "{\"name\":\"00af23602676b57bc1c0\",\"layouts\":[{\"id\":0,\"position\":{\"x\":3.9720713731574864,\"y\":305.84949573312645,\"z\":1500,\"width\":1140.9775019394879,\"height\":256.19860356865786,\"tabOrder\":0}}],\"singleVisual\":{\"visualType\":\"pivotTable\",\"projections\":{\"Values\":[{\"queryRef\":\"Referral Spells.m_CountReferralsSpells\"},{\"queryRef\":\"Referral Spells.m_ReferralSpells_AverageCaseLength_Days\"},{\"queryRef\":\"Caseloads (Simplified, By Service - Trial Balance LoD).m_Caseload_Simplified\"},{\"queryRef\":\"Contacts (Simplified).m_ContactsCount_Simplified_Attended\"},{\"queryRef\":\"Contacts (Simplified).m_ContactsCount_Simplified_PerOpenReferralPerMonth\"},{\"queryRef\":\"Trial Balance.m_WTEs\"},{\"queryRef\":\"Trial Balance.m_Cost_Pay_1000s\"},{\"queryRef\":\"Trial Balance.m_Cost_Pay_PerWTE_InPeriod\"},{\"queryRef\":\"Trial Balance.m_Caseload_PerCaseloadCarryingWTE\"},{\"queryRef\":\"Trial Balance.m_Cost_Pay_PerContact\"},{\"queryRef\":\"Trial Balance.m_Cost_Pay_PerCase\"},{\"queryRef\":\"Trial Balance.m_Caseload_PerConsultant\"}],\"Rows\":[{\"queryRef\":\"Acuity Tier.Service/Ward (Trial Balance LoD)\",\"active\":true}]},\"prototypeQuery\":{\"Version\":2,\"From\":[{\"Name\":\"t\",\"Entity\":\"Trial Balance\",\"Type\":0},{\"Name\":\"c\",\"Entity\":\"Caseloads (Simplified, By Service - Trial Balance LoD)\",\"Type\":0},{\"Name\":\"c1\",\"Entity\":\"Contacts (Simplified)\",\"Type\":0},{\"Name\":\"r1\",\"Entity\":\"Referral Spells\",\"Type\":0},{\"Name\":\"a\",\"Entity\":\"Acuity Tier\",\"Type\":0}],\"Select\":[{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"c\"}},\"Property\":\"m_Caseload_Simplified\"},\"Name\":\"Caseloads (Simplified, By Service - Trial Balance LoD).m_Caseload_Simplified\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"c1\"}},\"Property\":\"m_ContactsCount_Simplified_Attended\"},\"Name\":\"Contacts (Simplified).m_ContactsCount_Simplified_Attended\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"c1\"}},\"Property\":\"m_ContactsCount_Simplified_PerOpenReferralPerMonth\"},\"Name\":\"Contacts (Simplified).m_ContactsCount_Simplified_PerOpenReferralPerMonth\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"t\"}},\"Property\":\"m_Caseload_PerConsultant\"},\"Name\":\"Trial Balance.m_Caseload_PerConsultant\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"t\"}},\"Property\":\"m_Caseload_PerCaseloadCarryingWTE\"},\"Name\":\"Trial Balance.m_Caseload_PerCaseloadCarryingWTE\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"r1\"}},\"Property\":\"m_CountReferralsSpells\"},\"Name\":\"Referral Spells.m_CountReferralsSpells\"},{\"Column\":{\"Expression\":{\"SourceRef\":{\"Source\":\"a\"}},\"Property\":\"Service/Ward (Trial Balance LoD)\"},\"Name\":\"Acuity Tier.Service/Ward (Trial Balance LoD)\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"r1\"}},\"Property\":\"m_ReferralSpells_AverageCaseLength_Days\"},\"Name\":\"Referral Spells.m_ReferralSpells_AverageCaseLength_Days\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"t\"}},\"Property\":\"m_WTEs\"},\"Name\":\"Trial Balance.m_WTEs\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"t\"}},\"Property\":\"m_Cost_Pay_PerWTE_InPeriod\"},\"Name\":\"Trial Balance.m_Cost_Pay_PerWTE_InPeriod\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"t\"}},\"Property\":\"m_Cost_Pay_PerContact\"},\"Name\":\"Trial Balance.m_Cost_Pay_PerContact\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"t\"}},\"Property\":\"m_Cost_Pay_PerCase\"},\"Name\":\"Trial Balance.m_Cost_Pay_PerCase\"},{\"Measure\":{\"Expression\":{\"SourceRef\":{\"Source\":\"t\"}},\"Property\":\"m_Cost_Pay_1000s\"},\"Name\":\"Trial Balance.m_Cost_Pay_1000s\"}]},\"columnProperties\":{\"Caseloads (Simplified, By Service - Trial Balance LoD).m_Caseload_Simplified\":{\"displayName\":\"(C) Av. Caseload\"},\"Contacts (Simplified).m_ContactsCount_Simplified_Attended\":{\"displayName\":\"(D) Contacts (Attended)\"},\"Contacts (Simplified).m_ContactsCount_Simplified_PerOpenReferralPerMonth\":{\"displayName\":\"(E) Contacts Att. per SU per Month\"},\"Trial Balance.m_Caseload_PerConsultant\":{\"displayName\":\"(L) Av. Caseload per Consultant\"},\"Trial Balance.m_Caseload_PerCaseloadCarryingWTE\":{\"displayName\":\"(I) Av. Caseload per \\\"Caseload-Carrying\\\" WTE\"},\"Referral Spells.m_CountReferralsSpells\":{\"displayName\":\"(A) New Cases\"},\"Acuity Tier.Service/Ward (Trial Balance LoD)\":{\"displayName\":\"Service\"},\"Referral Spells.m_ReferralSpells_AverageCaseLength_Days\":{\"displayName\":\"(B) Av. Case Length (Days)\"},\"Trial Balance.m_WTEs\":{\"displayName\":\"(F) WTEs\"},\"Trial Balance.m_Cost_Pay_PerWTE_InPeriod\":{\"displayName\":\"(H) Cost per WTE in Period\"},\"Trial Balance.m_Cost_Pay_PerContact\":{\"displayName\":\"(J) Cost per Contact\"},\"Trial Balance.m_Cost_Pay_PerCase\":{\"displayName\":\"(K) Cost per Case\"},\"Trial Balance.m_Cost_Pay_1000s\":{\"displayName\":\"(G) Cost in Period (1000s)\"}},\"drillFilterOtherVisuals\":true,\"objects\":{\"columnWidth\":[{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"59.49209651596544D\"}}}},\"selector\":{\"metadata\":\"Caseloads (Simplified, By Service - Trial Balance LoD).m_Caseload_Simplified\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"74.11116890293953D\"}}}},\"selector\":{\"metadata\":\"Contacts (Simplified).m_ContactsCount_Simplified_Attended\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"75.91122091558509D\"}}}},\"selector\":{\"metadata\":\"Contacts (Simplified).m_ContactsCount_Simplified_PerOpenReferralPerMonth\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"94.90489812835744D\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Caseload_PerCaseloadCarryingWTE\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"77.60326541890497D\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Caseload_PerConsultant\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"73.37321209820736D\"}}}},\"selector\":{\"metadata\":\"Referral Spells.m_CountReferralsSpells\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"85.39791267390247D\"}}}},\"selector\":{\"metadata\":\"Referral Spells.m_ReferralSpells_AverageCaseLength_Days\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"234.3717168306803D\"}}}},\"selector\":{\"metadata\":\"Acuity Tier.Service/Ward (Trial Balance LoD)\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"71.51658838366285D\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Cost_Pay_PerWTE_InPeriod\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"72.26527511593498D\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Cost_Pay_PerContact\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"93.02094277231065D\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Cost_Pay_PerCase\"}},{\"properties\":{\"value\":{\"expr\":{\"Literal\":{\"Value\":\"63.53055023186997D\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Cost_Pay_1000s\"}}],\"columnFormatting\":[{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Caseloads (Simplified, By Service - Trial Balance LoD).m_Caseload_Simplified\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Contacts (Simplified).m_ContactsCount_Simplified_Attended\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Contacts (Simplified).m_ContactsCount_Simplified_PerOpenReferralPerMonth\"}},{\"properties\":{\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Caseload_PerCaseloadCarryingWTE\"}},{\"properties\":{\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Caseload_PerConsultant\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Referral Spells.m_CountReferralsSpells\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Referral Spells.m_ReferralSpells_AverageCaseLength_Days\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_WTEs\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Cost_Pay_PerWTE_InPeriod\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Cost_Pay_PerContact\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Cost_Pay_PerCase\"}},{\"properties\":{\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}},\"styleHeader\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}},\"styleTotal\":{\"expr\":{\"Literal\":{\"Value\":\"true\"}}}},\"selector\":{\"metadata\":\"Trial Balance.m_Cost_Pay_1000s\"}}],\"rowHeaders\":[{\"properties\":{\"fontFamily\":{\"expr\":{\"Literal\":{\"Value\":\"'''Segoe UI Bold'', wf_segoe-ui_bold, helvetica, arial, sans-serif'\"}}},\"fontSize\":{\"expr\":{\"Literal\":{\"Value\":\"8D\"}}}}}],\"columnHeaders\":[{\"properties\":{\"fontFamily\":{\"expr\":{\"Literal\":{\"Value\":\"'''Segoe UI Bold'', wf_segoe-ui_bold, helvetica, arial, sans-serif'\"}}},\"fontSize\":{\"expr\":{\"Literal\":{\"Value\":\"8D\"}}},\"alignment\":{\"expr\":{\"Literal\":{\"Value\":\"'Center'\"}}}}}],\"subTotals\":[{\"properties\":{\"fontFamily\":{\"expr\":{\"Literal\":{\"Value\":\"'''Segoe UI'', wf_segoe-ui_normal, helvetica, arial, sans-serif'\"}}},\"fontSize\":{\"expr\":{\"Literal\":{\"Value\":\"9D\"}}}}}],\"values\":[{\"properties\":{\"fontSize\":{\"expr\":{\"Literal\":{\"Value\":\"8D\"}}}}}],\"grid\":[{\"properties\":{\"rowPadding\":{\"expr\":{\"Literal\":{\"Value\":\"0D\"}}},\"gridVertical\":{\"expr\":{\"Literal\":{\"Value\":\"false\"}}},\"gridVerticalColor\":{\"solid\":{\"color\":{\"expr\":{\"ThemeDataColor\":{\"ColorId\":1,\"Percent\":0}}}}}}}],\"total\":[{\"properties\":{\"fontSize\":{\"expr\":{\"Literal\":{\"Value\":\"8D\"}}}}}]},\"vcObjects\":{\"title\":[{\"properties\":{\"show\":{\"expr\":{\"Literal\":{\"Value\":\"false\"}}},\"text\":{\"expr\":{\"Literal\":{\"Value\":\"'Productivity Metrics'\"}}},\"fontColor\":{\"solid\":{\"color\":{\"expr\":{\"Literal\":{\"Value\":\"'#666666'\"}}}}}}}],\"visualHeader\":[{\"properties\":{\"show\":{\"expr\":{\"Literal\":{\"Value\":\"false\"}}}}}]}},\"parentGroupName\":\"5aaf4ce9346ac815e90d\"}";
//var xx = JObject.Parse(test).ToString(Newtonsoft.Json.Formatting.Indented);
//var temp_folder = @"c:\test\temp";
//foreach (var originalPbix in pbiFiles)
//{
//    var x = PbiReportLoader.LoadFromPbiArchive(originalPbix);
//    x.DetectModelExtensions(out var exts);
//    x.ReplaceModelReferences(renames);
//    var str = x.Layout.ToString(Newtonsoft.Json.Formatting.Indented);
//}


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

