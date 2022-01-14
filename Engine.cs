using Packer.Model;
using Packer.Steps;
using Packer.Storage;
using System.IO.Compression;

namespace Packer
{
    /// <summary>
    /// Class that allows running a series of transformation steps on the repository.
    /// It defines all the steps needed to convert files from machine to human readable
    /// form and back again.
    /// </summary>
    internal class Engine
    {
        List<StepBase> steps = new List<StepBase>();

        public Engine()
        {
            steps.Add(new StripSecurityStep());
            steps.Add(new StripTimestapsStep());
            steps.Add(new ExtractTablesStep());
            steps.Add(new ExtractPagesStep());
            steps.Add(new SetSchemasStep());
            steps.Add(new ResolveVariablesStep());
        }

        public void Extract(string pbitFile, string repositoryFolder)
        {
            var folderStore = new FolderFilesStore(repositoryFolder);
            folderStore.UpdateContentsFromZip(pbitFile);

            var repoModel = new RepositoryModel(folderStore);
            foreach(var step in steps)
                step.ToHumanReadable(repoModel);

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
            
            // traverse steps in reverse order
            for(int i = steps.Count-1; i >=0; i--)
                steps[i].ToMachineReadable(model);
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
