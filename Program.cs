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
                    engine.Pack(args[1], args[2]);
                    break;
                case "unpack":
                    engine.Extract(args[1], args[2]);
                    break;
            }
        }
    }
}