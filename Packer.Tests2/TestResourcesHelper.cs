using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Packer.Tests2
{
    internal static class TestResourcesHelper
    {
        public static string GetTestModelContents()
        {
            return GetTestFileContents("test_model.bim");
        }

        public static string GetSimpleTestModelContents()
        {
            return GetTestFileContents("test_model_simple.bim");
        }

        public static string GetTestFileContents(string fileName)
        {
            using (var stream = typeof(TestResourcesHelper).Assembly.GetManifestResourceStream($"Packer.Tests2.TestFiles.{fileName}"))
                return new StreamReader(stream).ReadToEnd();
        }
    }
}
