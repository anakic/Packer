//using Newtonsoft.Json.Linq;
//using System.Text;

//namespace Packer.Steps
//{
//    internal class SetSchemasStep : StepBase
//    {
//        public override void Pack(string folderPath, string pbitFilePath)
//        {
//            Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
//                .ToList()
//                .ForEach(f =>
//                {
//                    if (TryGetSchema(f, out var schemaPath))
//                    {
//                        var str = File.ReadAllText(f);
//                        var obj = JObject.Parse(str);
//                        obj.Root.Append(new JProperty("$schema", schemaPath));
//                        File.WriteAllText(f, obj.ToString());
//                    }
//                });

//            base.Pack(folderPath, pbitFilePath);
//        }

//        public override void Extract(string pbitFilePath, string folderPath)
//        {
//            Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
//                .ToList()
//                .ForEach(f =>
//                {
//                    if (TryGetSchema(f, out var schemaPath))
//                    {
//                        var str = File.ReadAllText(f);
//                        var obj = JObject.Parse(str);
//                        obj.Descendants().OfType<JProperty>().Where(d => d.Name == "$schema").ToList().ForEach(c => c.Remove());
//                        var bytes = Encoding.Unicode.GetBytes(obj.ToString());
//                        File.WriteAllBytes(f, bytes);
//                    }
//                });

//            base.Extract(pbitFilePath, folderPath);
//        }

//        private bool TryGetSchema(string filePath, out string schemaPath)
//        {
//            schemaPath = "";
//            return false;
//        }
//    }
//}
