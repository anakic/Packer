using Packer2.Library;
using Packer2.Library.DataModel;
using Microsoft.Extensions.Logging;
using Packer2.Library.Report.Stores.Folder;
using DataModelLoader.Report;

namespace Packer.ManualTestingArea
{
    public class Class2
    {
        static Guid groupId = Guid.Parse("b060ead8-e4b4-4855-8ee1-633dddbd796c");
        static string appId = "efb3ae81-eb15-41de-ad47-01bb8370f5e8";
        static string tenantId = "9ac94aa1-d615-4174-8367-c2208fb31f6b";
        static string secret = "jTN8Q~-l73JaFx54AJqkWBnidTQP-6WYIxwWHaD-";

        public static void Main()
        {
            var pbiArch = new PBIArchiveStore(@"C:\Dropbox (RwHealth)\Flow Tool - DHFT\AN DHCFT Backup\Inequalities.pbix");
            var xx = pbiArch.Read();

            

            var rfs = new ReportFolderStore("C:\\Dropbox (RwHealth)\\Flow Tool - DHFT\\AN DHCFT Backup\\Inequalities_2");
            rfs.Save(xx);
            rfs.Read();

            return;

            var db = TabularModel.LoadFromFolder(@"C:\GH\Data Model");
            //db.DeclareDataSources();

            //db.Model.Expressions.ToList().ForEach(exp =>
            //{
            //    if (db.Model.Tables.Contains(exp.Name))
            //    {
            //        var matchingTable = db.Model.Tables[exp.Name];
            //        exp.Name = exp.Name + "_expr";
            //        matchingTable.Partitions.Select(p => p.Source).OfType<MPartitionSource>().ToList().ForEach(ps =>
            //        {
            //            string origExpression = ps.Expression;
            //            ps.Expression = ps.Expression.Replace($@"#""{matchingTable.Name}""", $@"#""{exp.Name}""");
            //            Trace.WriteLine($">>> Table {matchingTable.Name}: replacing expression <exp>{origExpression.Replace("\r\n", " ")}</exp> with <exp>{ps.Expression.Replace("\r\n", " ")}</exp>.");
            //        });
            //    }
            //});

            //db.SaveToFolder(@"C:\GH\Data Model");

            // Create LoggerFactory
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PowerBIServiceDataModelStore>();

            var store23 = new PowerBIServiceDataModelStore($"powerbi://api.powerbi.com/v1.0/myorg/DHCFT%20%7C%20DEV", "dhcft_packer_test", groupId, tenantId, appId, secret, AutoProcessBehavior.Sequential, logger: logger);
            store23.Save(db);
        }
    }
}
