using DataModelLoader;
using DataModelLoader.Report;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Packer.Tests2
{
    public class ReportParseTests
    {
        [Fact]
        public void ParsesReport()
        {
            var loader = new PBIArchiveStore(@"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\ward_flow3.pbit");
            var model = loader.Read();

            var folderStore = new PBIFolderLoader(@"c:\Models\test");
            folderStore.Save(model);

            //string filePath = @"C:\Models\ward_flow3_pbitunzip\Report\Layout";
            //var report = Newtonsoft.Json.JsonConvert.DeserializeObject<ReportLayout>(File.ReadAllText(filePath));
        }
    }
}
