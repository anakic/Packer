using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Packer
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("Packer", LogLevel.Debug)
                       .AddConsole();
            });

            Engine engine = new Engine(loggerFactory);

            //string file = @"C:\TEST_PBI_VC\DHFT-MHFT v1.1.7 - Development Master.pbit";
            //string outputFile = file.Substring(0, file.LastIndexOf(".")) + "_out.pbit";
            //engine.Extract(file, @"C:\TEST_PBI_VC\repo");
            //engine.Pack(@"C:\TEST_PBI_VC\repo", outputFile);
            //return;

            var operation = args[0];
            switch (operation)
            {
                case "pack":
                    if (args.Count() == 3)
                        engine.Pack(args[1], args[2]);
                    else if (args.Count() == 2)
                        engine.Pack(Environment.CurrentDirectory, args[1]);
                    else
                        throw new ArgumentException();
                    break;
                case "unpack":
                    if (args.Count() == 3)
                        engine.Extract(args[1], args[2]);
                    else if (args.Count() == 2)
                        engine.Extract(args[1], Environment.CurrentDirectory);
                    else
                        throw new ArgumentException();
                    break;
            }
        }
    }
}