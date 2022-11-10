// See https://aka.ms/new-console-template for more information
using Antlr4.Runtime;
using Packer2.Library.MinifiedQueryParser;
using Packer2.Library.Tools;

string input = @"from d in Dates,
d1 in [dbth Spell]
orderby d.Date_Invalid ascending
select d.Date_Invalid, d1.SpellsInPeriod, d1.IsFromED";

var parser = new QueryParser(new DummyLogger<QueryParser>());
var res = parser.ParseQuery(input);

var x = Newtonsoft.Json.JsonConvert.SerializeObject(res, Newtonsoft.Json.Formatting.Indented);

Console.Write(res.ToString());
Console.ReadLine();

