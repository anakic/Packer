// See https://aka.ms/new-console-template for more information
using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Packer2.Library.Report.Transforms;
using Packer2.Library.Tools;

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

var rfs = new PBIArchiveStore(@"c:\test\samo_jedan_visual.pbix");
var report = rfs.Read();


report = new MarkerTransform().Transform(report);
report = new PrettifyModelExpressionsTransform().Transform(report);
report = new RestoreModelExpressionsTransform(new DummyLogger<RestoreModelExpressionsTransform>()).Transform(report);

var folderStore = new ReportFolderStore(@"c:\test\sjv");
folderStore.Save(report);
//var report = folderStore.Read();

var rfs2 = new PBIArchiveStore(@"c:\test\sjv.pbix");
rfs2.Save(report);


// rfs.Save(report);

