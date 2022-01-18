namespace Packer
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            Engine engine = new Engine();
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