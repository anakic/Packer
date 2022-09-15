// See https://aka.ms/new-console-template for more information
using Microsoft.AnalysisServices;

var s = new Server();
s.Connect($"Data source=localhost:54287");
var t = string.Join (";", s.Databases[0].Model.Tables.Select(t => t.Name));
var info = s.ConnectionInfo;
Console.Write(t);