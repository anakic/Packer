using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var s = new Server();
            s.Connect($"Data source=localhost:54287");
            var t = s.Databases[0].Model.Tables.Count;
            
            s.Databases[0].Update()
            var info = s.ConnectionInfo;
        }
    }
}
