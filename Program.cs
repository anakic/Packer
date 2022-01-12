using Newtonsoft.Json.Linq;
using Packer.Steps;
using Packer.Tools;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Packer
{
    internal static class Program
    {
        private static readonly Encoding Encoding = Encoding.Unicode;

        private static StepBase? firstStep;
        private static StepBase? lastStep;

        private static void AddStep(StepBase step)
        {
            if (firstStep == null)
                firstStep = step;

            if (lastStep != null)
                lastStep.Next = step;

            lastStep = step;
        }

        public static void Main(string[] args)
        {
            AddStep(new ZipStep());
            AddStep(new ToUnicodeStep());
            AddStep(new ReformatJsonFilesStep());
            AddStep(new ResolveVariablesStep());
            AddStep(new StripSecurityStep());
            AddStep(new StripTimestapsStep());
            AddStep(new ExtractTablesStep());
            //AddStep(new SetSchemasStep());

            firstStep?.Pack(@"C:\TEST_PBI_VC\unpacked", @"C:\TEST_PBI_VC\aw_sales.pbit");
            // firstStep?.Extract(@"C:\TEST_PBI_VC\aw_sales.pbit", @"C:\TEST_PBI_VC\unpacked");
            return;

            var operation = args[0];
            switch (operation)
            {
                case "pack":
                    firstStep?.Pack(args[1], args[2]);
                    break;
                case "unpack":
                    firstStep?.Extract(args[1], args[2]);
                    break;
            }
        }
    }
}