using CommandLine.Text;
using CommandLine;

class Program
{
    public enum ModelOutputKind
    {
        Bim,
        Pbit,
        Folder,
        SSAS,
        Auto
    }

    [Verb("process-model")]
    public class ModelOptions
    {
        [Value(0, Required = true)]
        public string? ModelLocation { get; set; }

        [Option('o', "out", Required = false, HelpText = "Set output location.")]
        public string? OutPath { get; set; }

        [Option('k', "kind", Required = false, HelpText = "Set output location.", Default = ModelOutputKind.Auto)]
        public ModelOutputKind OutputKind { get; set; }
    }

    [Verb("process-report")]
    public class ReportOptions
    {
        [Value(0)]
        public string ReportPath { get; set; }

        [Option('o', "out", Required = false, HelpText = "Set output location.")]
        public string? OutPath { get; set; }

        [Option('k', "kind", Required = false, HelpText = "Set output location.", Default = ModelOutputKind.Auto)]
        public ModelOutputKind OutputKind { get; set; }
    }

    static void Main(string[] args)
    {
        var x = Parser.Default.ParseArguments<ModelOptions, ReportOptions>(args)
            .WithParsed<ReportOptions>(o =>
            {

            })
            .WithParsed<ModelOptions>(o =>
            {

            });

        Console.ReadLine();
    }
}