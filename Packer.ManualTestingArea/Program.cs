﻿// See https://aka.ms/new-console-template for more information
using Antlr4.Runtime;
using DataModelLoader.Report;
using Microsoft.Extensions.Logging;
using Packer2.Library.MinifiedQueryParser;
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

string str = @"{from d in Ward
orderby d.TypeKind ascending
select d.TypeKind }.TypeKind";

var str2 = "from d in Dates\r\nselect d.hierarchy([Date Hierarchy]).level(Year), d.hierarchy([Date Hierarchy]).level(Month), d.hierarchy([Date Hierarchy]).level(Date)";

var p1 = new QueryParser(loggerFactory.CreateLogger<QueryParser>());
var q = p1.ParseQuery(str2);


var rfs = new ReportFolderStore(@"c:\test\aa");
var report = rfs.Read();
report = new RestoreModelExpressionsTransform(loggerFactory.CreateLogger<RestoreModelExpressionsTransform>()).Transform(report);
// rfs.Save(report);

Console.ReadLine();

