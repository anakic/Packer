using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer
{
    public class API
    {
        Engine engine;

        public string CurrentDirectory { get; set; }

        public API()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("Packer", LogLevel.Debug)
                       .AddConsole();
            });
            engine = new Engine(loggerFactory);

            CurrentDirectory = Environment.CurrentDirectory;
        }

        public void Unpack(string inPbiFilePath) 
        {
            engine.Extract(inPbiFilePath, CurrentDirectory);
        }

        public void Pack(string outPbiFilePath) 
        {
            engine.Pack(CurrentDirectory, outPbiFilePath);
        }

        public void UnpackModel(string inBimFilePath) { }
        public void PackModel(string outBimFilePath) { }

        public void MigrateModel(string outModelFolderPath) { }
    }
}
