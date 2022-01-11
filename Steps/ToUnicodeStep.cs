using Packer.Tools;
using System.Text;

namespace Packer.Steps
{
    internal class ToUnicodeStep : StepBase
    {
        public override void Extract(string pbitFilePath, string folderPath)
        {
            JsonFileHelpers.GetAllJsonFiles(folderPath)
                .ToList().ForEach(f =>
                {
                    var allBytes = File.ReadAllBytes(f);
                    var obj = JsonFileHelpers.ParseJsonStr(allBytes);
                    File.WriteAllText(f, obj.ToString());
                });

            base.Extract(pbitFilePath, folderPath);
        }

        public override void Pack(string folderPath, string pbitFilePath)
        {
            base.Pack(folderPath, pbitFilePath);

            JsonFileHelpers.GetAllJsonFiles(folderPath)
                .ToList().ForEach(f =>
                {
                    var text = File.ReadAllText(f);
                    var bytes = Encoding.Unicode.GetBytes(text);
                    File.WriteAllBytes(f, bytes);
                });
        }
    }
}
