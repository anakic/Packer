using System.IO.Compression;

namespace Packer.Steps
{
    internal class ZipStep : StepBase
    {
        public override void Extract(string pbitFilePath, string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                // clear everything except the .git folder
                foreach (var d in Directory.GetDirectories(folderPath))
                {
                    if (Path.GetFileName(d) == ".git")
                        continue;
                    Directory.Delete(d, true);
                }
                foreach (var f in Directory.GetFiles(folderPath))
                {
                    File.Delete(f);
                }
            }
            else
                Directory.CreateDirectory(folderPath);

            ZipFile.ExtractToDirectory(pbitFilePath, folderPath);

            base.Extract(pbitFilePath, folderPath);
        }

        public override void Pack(string folderPath, string pbitFilePath)
        {
            var tempFolderPath = Path.Combine(Path.GetTempPath(), "Packer_temp");

            if (Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);

            foreach (var filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                if (filePath.Contains(".git\\"))
                    continue;

                var relativePath = filePath[(folderPath.TrimEnd('\\').Length + 1)..];
                var destinationPath = Path.Combine(tempFolderPath, relativePath);

                var destinationDir = Path.GetDirectoryName(destinationPath) ?? throw new Exception("Invalid path");
                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                File.Copy(filePath, destinationPath);
            }

            base.Pack(tempFolderPath, pbitFilePath);

            if (File.Exists(pbitFilePath))
                File.Delete(pbitFilePath);
            ZipFile.CreateFromDirectory(tempFolderPath, pbitFilePath);

            Directory.Delete(tempFolderPath, true);
        }
    }
}
