using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Packer.Tests
{
    public class Class1
    {
        [Fact]
        public void MigratesToSSAS()
        {
            var file1= @"C:\Models\x\SABP Mental Health Flow Tool - v1.2.17 Development Master\DataModelSchema";
            var file1_out = @"C:\Models\x\SABP Mental Health Flow Tool - v1.2.17 Development Master\DataModelSchema_out";
            
            var file2 = @"C:\Users\AntonioNakic-Alfirev\OneDrive - SSG Partners Limited\Desktop\New folder (4)\model.adjusted.json";

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("Packer", LogLevel.Debug)
                       .AddConsole();
            });
            var engine = new Engine(loggerFactory);

            engine.MigrateToSSAS(file1, file1_out);

            //BimProcessor bp = new BimProcessor();
            //var res = bp.AdjustBim(File.ReadAllText(file1));

            //res.GetHashCode().Should().Be(File.ReadAllText(file2).GetHashCode());
        }
    }
}