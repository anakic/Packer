using Packer.Model;
using Packer.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer
{
    internal class Engine
    {
        private static StepBase? firstStep;
        private static StepBase? lastStep;

        private static void AddStep(StepBase step)
        {
            if (firstStep == null)
                firstStep = step;

            if (lastStep != null)
                lastStep.Next = step;

            lastStep = step;
        }

        public void Extract(string pbitFile, string repositoryFolder)
        {
            var repoModel = new RepositoryModel(repositoryFolder);
            repoModel.Load(pbitFile);

            firstStep?.Extract(repoModel);
        }

        public void Pack(string repositoryFolder, string pbitFile)
        {
            var tempFolderPath = Path.Combine(Path.GetTempPath(), "Packer_temp");

            if (Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);

            // copy everything except the git folder to a temp folder the steps can
            // make changes without affecting the working folder of the git repo
            foreach (var filePath in Directory.GetFiles(repositoryFolder, "*", SearchOption.AllDirectories))
            {
                if (filePath.Contains(".git\\"))
                    continue;

                var relativePath = Utils.GetRelativePath(repositoryFolder, filePath);
                var destinationPath = Path.Combine(tempFolderPath, relativePath);

                var destinationDir = Path.GetDirectoryName(destinationPath)!;
                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                File.Copy(filePath, destinationPath);
            }

            // create a new model at the temp location
            var model = new RepositoryModel(tempFolderPath);
            // run the pack transformations
            firstStep?.Pack(model);
            // generate pbit file
            model.GeneratePbit(pbitFile);
            // remove temp dir
            Directory.Delete(tempFolderPath, true);
        }
    }
}
