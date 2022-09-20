using DataModelLoader.Report;
using FluentAssertions;
using Microsoft.AnalysisServices.Tabular;
using Xunit;

namespace Packer2.Tests.Report
{
    public class ReportParseTests
    {
        [Fact]
        public void ParsesReport()
        {
            string archivePath = @"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit";
            var initialBytes = File.ReadAllBytes(archivePath);

            var store1 = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit");
            var model = store1.Read();

            string outputPath = @"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3-updated.pbit";
            var store2 = new PBIArchiveStore(outputPath);
            store2.Save(model);
            var outputBytes = File.ReadAllBytes(outputPath);

            initialBytes.Should().NotBeEquivalentTo(outputBytes);
        }

        [Fact]
        public void SavesToFolder()
        {
            var store1 = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit");
            var model = store1.Read();
            var folderStore = new ReportFolderStore(@"c:\Models\test");
            folderStore.Save(model);
        }

        [Fact]
        public void ReadFromFolder()
        {
            var folderStore = new ReportFolderStore(@"c:\Models\test");
            var model = folderStore.Read();

            var pbitStore = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3-out.pbit");
            pbitStore.Save(model);
        }
    }
}
