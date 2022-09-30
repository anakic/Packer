using DataModelLoader.Report;
using FluentAssertions;
using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using Packer2.Library.DataModel;
using Packer2.Library;
using Packer2.Library.Report.Transforms;
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
            var store1 = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3-out-conn.pbix");
            var model = store1.Read();
            var folderStore = new ReportFolderStore(@"c:\Models\test2");
            folderStore.Save(model);
        }

        [Fact]
        public void ReadFromFolder()
        {
            var folderStore = new ReportFolderStore(@"c:\Models\test");
            var model = folderStore.Read();

            var transform = new SwitchDataSourceToSSASTransform("Data Source=localhost:53682;Initial Catalog=f6bb0b1f-4a66-4bbd-8cff-44f69d71b6eb;Cube=Model");
            transform.Transform(model);

            var pbitStore = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3-out.pbix");
            pbitStore.Save(model);
        }

        [Fact]
        public void TEMP_MigrateTest()
        {
            var ReportLocation = @"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3 - Copy.pbit";
            var SsasServer = ".";
            int? SsasVersion = null;
            var SsasDatabaseName = "DB";
            var CodeOutputPath = @"c:\models\ps\test_migrate_ssas_datamodel";

            IModelStore<PowerBIReport> store;
            store = new PBIArchiveStore(ReportLocation);
            
            Lazy<Server> lazyServer = new Lazy<Server>(() =>
            {
                var srv = new Server();
                srv.Connect(SsasServer);
                return srv;
            });

            int version;
            if (SsasVersion.HasValue)
                version = SsasVersion.Value;
            else
            {
                version = lazyServer.Value.SupportedCompatibilityLevels.Split(",").Where(x => x.Length == 4/*removing the 100000 entry*/).Select(Int32.Parse).Max();
            }

            var reportModel = store.Read();

            var dataModelStore = new BimDataModelStore(new MemoryFile(reportModel.DataModelSchemaFile!.ToString()));
            var dataModel = dataModelStore.Read();
            var dataTransforms = new IDataModelTransform[]
            {
                new DowngradeTransform(version),
                new RegisterDataSourcesTransform(),
                new PullUpExpressionsTranform(),

                // todo: add switches to optionally add these steps?
                // commented out because this isn't a necessary part of deployment, it's something to optionally do manually
                new StripLocalDateTablesTransform(),
                new StripCulturesTransform(),
            };
            foreach (var t in dataTransforms)
                dataModel = t.Transform(dataModel);

            // save code to folder
            if (CodeOutputPath != null)
            {
                var outputDataModelStore = new FolderModelStore(CodeOutputPath);
                outputDataModelStore.Save(dataModel);
            }

            // deploy ssas instance
            if (SsasServer != null)
            {
                // var serverStore = new BimDataModelStore(new LocalTextFile(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\test.bim"));
                var serverStore = new SSASDataModelStore(SsasServer, SsasDatabaseName);
                serverStore.Save(dataModel);
            }

            // remove the datamodelschema from the powerbi file and add the SSAS connection
            string? connectionString = null;
            if (SsasServer != null)
                connectionString = $"Data source = {SsasServer};";
            if (SsasDatabaseName != null)
                connectionString += $"Initial catalog={SsasDatabaseName}";
            var reportTransforms = new IReportTransform[]
            {
                new SwitchDataSourceToSSASTransform(connectionString)
            };

            // todo: save back to report source (pbit or folder)
        }
    }
}
