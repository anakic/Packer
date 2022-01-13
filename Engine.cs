using Packer.Model;
using Packer.Steps;
using Packer.Storage;
using System.IO.Compression;

namespace Packer
{
    internal class Engine
    {
        private StepBase? firstStep;
        private StepBase? lastStep;

        public void AddStep(StepBase step)
        {
            if (firstStep == null)
                firstStep = step;

            if (lastStep != null)
                lastStep.Next = step;

            lastStep = step;
        }

        public void Extract(string pbitFile, string repositoryFolder)
        {
            var folderStore = new FolderFilesStore(repositoryFolder);
            folderStore.UpdateContentsFromZip(pbitFile);

            var repoModel = new RepositoryModel(folderStore);
            firstStep?.ToHumanReadable(repoModel);

            folderStore.ClearContents();
            repoModel.WriteTo(folderStore, true);
        }

        public void Pack(string repositoryFolder, string pbitFile)
        {
            // prepare temp dir
            var tempFolderPath = Path.Combine(Path.GetTempPath(), "Packer_temp");
            if (Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);

            // read and modify model, then save to output folder
            var store = new FolderFilesStore(repositoryFolder);
            RepositoryModel model = new RepositoryModel(store);
            firstStep?.Pack(model);
            model.WriteTo(new FolderFilesStore(tempFolderPath), false);

            // create zip from output folder
            if (File.Exists(pbitFile))
                File.Delete(pbitFile);
            ZipFile.CreateFromDirectory(tempFolderPath, pbitFile);

            // remove temp dir
            Directory.Delete(tempFolderPath, true);
        }
    }
}
