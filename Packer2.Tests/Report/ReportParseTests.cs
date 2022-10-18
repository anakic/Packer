using DataModelLoader.Report;
using FluentAssertions;
using Packer2.Library;
using Packer2.Library.DataModel;
using Packer2.Library.Report.Transforms;
using System.Diagnostics;
using Xunit;
using static Packer2.Library.Report.Transforms.ValidateModelReferencesTransform;

namespace Packer2.Tests.Report
{
    public class ReportParseTests
    {
        //[Fact]
        //public void TEMP()
        //{
        //    var wf3Pbit = @"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit";
        //    var reportModel = new PBIArchiveStore(wf3Pbit).Read();
        //    var db = new BimDataModelStore(new JObjFile(reportModel.DataModelSchemaFile)).Read();

        //    new ValidateModelReferencesTransform(db).Transform(reportModel);
        //}

        //[Fact]
        //public void ParsesReport()
        //{
        //    string archivePath = @"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit";
        //    var initialBytes = File.ReadAllBytes(archivePath);

        //    var store1 = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit");
        //    var model = store1.Read();

        //    string outputPath = @"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3-updated.pbit";
        //    var store2 = new PBIArchiveStore(outputPath);
        //    store2.Save(model);
        //    var outputBytes = File.ReadAllBytes(outputPath);

        //    initialBytes.Should().NotBeEquivalentTo(outputBytes);
        //}

        //[Fact]
        //public void SavesToFolder()
        //{
        //    var store1 = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit");
        //    var model = store1.Read();
        //    var folderStore = new ReportFolderStore(@"c:\Models\test2");
        //    folderStore.Save(model);

        //    var folderStore2 = new ReportFolderStore(@"c:\Models\test2");
        //    var model2 = folderStore.Read();
        //}

        //[Fact]
        //public void ReadFromFolder()
        //{
        //    var folderStore = new ReportFolderStore(@"c:\Models\test");
        //    var model = folderStore.Read();

        //    var transform = new SwitchDataSourceToSSASTransform("Data Source=localhost:53682;Initial Catalog=f6bb0b1f-4a66-4bbd-8cff-44f69d71b6eb;Cube=Model");
        //    transform.Transform(model);

        //    var pbitStore = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3-out.pbix");
        //    pbitStore.Save(model);
        //}
    }
}
