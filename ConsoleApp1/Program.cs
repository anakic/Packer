// See https://aka.ms/new-console-template for more information
using Microsoft.AnalysisServices;

Console.WriteLine("Hello, World!");


var s = new Server();
s.Connect("localhost:54287");
return s.Databases[0].Model.Tables.Count;