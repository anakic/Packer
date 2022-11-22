// See https://aka.ms/new-console-template for more information
using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Packer2.Library.Report.Stores.Folder;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Debug)
        .AddFilter("System", LogLevel.Debug)
        .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
        .AddConsole();
});

//string str = @"{from d in Ward
//orderby d.TypeKind ascending
//select d.TypeKind }.TypeKind";

//var str2 = @"(Crisis.m_CountCrisisExit3Parallel / ScopedEval(Crisis.m_CountCrisisExit3Parallel, Scope(roleRef[Columns])))";

//var p1 = new QueryParser(loggerFactory.CreateLogger<QueryParser>());
//var q = p1.ParseExpression(str2);

var rfs = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit");
var report = rfs.Read();

var folderStore = new ReportFolderStore(@"c:\test\wf3_orig");
folderStore.Save(report);
//var report = folderStore.Read();

//var rfs2 = new PBIArchiveStore(@"c:\test\sjv.pbix");
//rfs2.Save(report);


// rfs.Save(report);

