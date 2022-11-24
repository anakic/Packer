// See https://aka.ms/new-console-template for more information
using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json;
using Packer2.Library.MinifiedQueryParser.QueryTransforms;
using Packer2.Library.Report.QueryTransforms.Antlr;
using Packer2.Library.Report.Stores.Folder;
using Packer2.Library.Tools;
using System.Diagnostics;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Debug)
        .AddFilter("System", LogLevel.Debug)
        .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
        .AddConsole();
});

string folder = @"C:\TEst\raspakirano_sabp";
string raspakiranoPbix = @"C:\TEst\raspakirano_sabp.pbix";
string wf3_ssas = @"C:\TEst\wf3_ssas.pbix";
string wf3pbit = @"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit";
string sjvpbix = @"C:\TEst\samo_jedan_visual.pbix";

//string str = @"{from d in Ward
//orderby d.TypeKind ascending
//select d.TypeKind }.TypeKind";

//var str2 = @"(Crisis.m_CountCrisisExit3Parallel / ScopedEval(Crisis.m_CountCrisisExit3Parallel, Scope(roleRef[Columns])))";

//var p1 = new QueryParser(loggerFactory.CreateLogger<QueryParser>());
//var q = p1.ParseExpression(str2);

//var rfs = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit");
//var report = rfs.Read();

//var folderStore = new ReportFolderStore(@"c:\test\wf3_orig");
//folderStore.Save(report);

// 1. procitaj
var rfs = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\Downloads\SABP Mental Health Flow Tool - v1.2.17 Development Master.pbix");
var report = rfs.Read();

//// 2. spremi minificirano
var folderStore = new ReportFolderStore(folder);
folderStore.EnableMinification();
folderStore.Save(report);

//// 3. deminificiraj u folderu
report = folderStore.Read();
folderStore.DisableMinification();
folderStore.Save(report);

// 4. procitaj tako deminificirano i zapakiraj
report = folderStore.Read();
var rfs2 = new PBIArchiveStore(raspakiranoPbix);
rfs2.Save(report);

// 5. upali i testiraj
Process.Start("explorer.exe", raspakiranoPbix);

//report = folderStore.Read();

//var rfs2 = new PBIArchiveStore(@"C:\TEst\samo_jedan_visual2.pbix");
//rfs2.Save(report);
//var report = folderStore.Read();

//var rfs2 = new PBIArchiveStore(@"c:\test\sjv.pbix");
//rfs2.Save(report);


// rfs.Save(report);

