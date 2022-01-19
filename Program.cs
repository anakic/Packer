namespace Packer
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Engine engine = new Engine();

            engine.Extract(@"C:\TEST_PBI_VC\CNWL Flow Model v3.7.2 - JP Optimisation.pbit", @"C:\TEST_PBI_VC\repo");
            engine.Pack(@"C:\TEST_PBI_VC\repo", @"C:\TEST_PBI_VC\test.pbit");
            return;

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
                    if(args.Count() == 3)
                    engine.Extract(args[1], args[2]);
                    else if(args.Count() == 2)
                        engine.Extract(args[1], Environment.CurrentDirectory);
                    else
                        throw new ArgumentException();
                    break;
            }
        }
    }
}