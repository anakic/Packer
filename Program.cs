using Newtonsoft.Json.Linq;
using Packer.Steps;
using Packer.Tools;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace Packer
{
    /*
     * 
     * The model exposes objects for files and folders. Steps modify these objects.
     * These objects are loaded from disk on load and saved to disk explicitly by calling Save() on the model.
     * No other interaction with the files on disk is allowed.
     
    1. json files should have a Formatting property
    2. all files should have a save changes method
    3. steps should only deal with json/xml/text, not with bytes so fileitems should not expose raw data (these methods should be protected instead of public)
     
     */



    internal static class Program
    {
        public static void Main(string[] args)
        {
            Engine engine = new Engine();
            engine.AddStep(new ReformatJsonFilesStep());
            engine.AddStep(new StripSecurityStep());
            engine.AddStep(new StripTimestapsStep());
            engine.AddStep(new ExtractTablesStep());
            engine.AddStep(new ResolveVariablesStep());
            //AddStep(new SetSchemasStep());

            engine.Pack(@"C:\TEST_PBI_VC\unpacked", @"C:\TEST_PBI_VC\aw_sales.pbit");
            // firstStep?.Extract(@"C:\TEST_PBI_VC\aw_sales.pbit", @"C:\TEST_PBI_VC\unpacked");
            return;

            var operation = args[0];
            switch (operation)
            {
                case "pack":
                    engine.Pack(args[1], args[2]);
                    break;
                case "unpack":
                    engine.Extract(args[1], args[2]);
                    break;
            }
        }
    }
}