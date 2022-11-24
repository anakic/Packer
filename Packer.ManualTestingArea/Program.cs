﻿// See https://aka.ms/new-console-template for more information
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

string pbix = @"C:\TEst\wf3_ssas.pbix";
string folder = @"C:\TEst\unpacked";
string repackedPbix = @"C:\TEst\repacked.pbix";

// 1. procitaj
var rfs = new PBIArchiveStore(pbix);
var report = rfs.Read();

//// 2. spremi minificirano
var folderStore = new ReportFolderStore(folder);
folderStore.EnableQueryMinification = true;
folderStore.Save(report);

//// 3. deminificiraj u folderu
report = folderStore.Read();
//folderStore.EnableQueryMinification = false;
//folderStore.Save(report);

// 4. procitaj tako deminificirano i zapakiraj
report = folderStore.Read();
var rfs2 = new PBIArchiveStore(repackedPbix);
rfs2.Save(report);

// 5. upali i testiraj
Process.Start("explorer.exe", repackedPbix);
