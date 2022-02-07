using Microsoft.Extensions.Logging;
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
    public class Engine
    {
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        List<StepBase> steps = new List<StepBase>();

        public Engine(ILoggerFactory loggerFactory)
        {
            steps.Add(new StripSecurityStep());
            steps.Add(new StripTimestapsStep());
            steps.Add(new ExtractTablesStep());
            steps.Add(new ExtractDaxStep());
            steps.Add(new ExtractMStep());
            steps.Add(new ExtractPagesStep());
            steps.Add(new ConsolidateVisualsOrderingStep());
            steps.Add(new OrderArraysStep());
            steps.Add(new StripPageGenPropsSteps());
            steps.Add(new UnstuffJsonStep());
            steps.Add(new ExtractBookmarksStep()); // must be after unstuff because bookmarks are stuffed inside layout/config property
            steps.Add(new SetSchemasStep(loggerFactory.CreateLogger<SetSchemasStep>()));
            steps.Add(new ResolveVariablesStep());
            this.loggerFactory = loggerFactory;
            logger = loggerFactory.CreateLogger<Engine>();
        }

        public void Extract(string pbitFile, string repositoryFolder)
        {
            logger.LogInformation($"Extracting file '{pbitFile}' to folder '{repositoryFolder}'");
            var folderStore = new FolderFilesStore(repositoryFolder);

            logger.LogDebug($"Unzipping...");
            folderStore.UpdateContentsFromZip(pbitFile);

            var repoModel = new RepositoryModel(folderStore);
            foreach (var step in steps)
            {
                logger.LogDebug($"Running step {step.GetType().Name}...");
                step.ToHumanReadable(repoModel);
            }

            folderStore.ClearContents();
            repoModel.WriteTo(folderStore, true);
            logger.LogInformation($"Extracting complete!");
        }

        public void Pack(string repositoryFolder, string pbitFile)
        {
            logger.LogInformation($"Packing folder '{repositoryFolder}' into file '{pbitFile}'...");

            // prepare temp dir
            logger.LogDebug($"Preparing temp output folder...");
            var tempFolderPath = Path.Combine(Path.GetTempPath(), "Packer_temp");
            if (Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);

            var store = new FolderFilesStore(repositoryFolder);
            RepositoryModel model = new RepositoryModel(store);
            // traverse steps in reverse order
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                var step = steps[i];
                logger.LogDebug($"Running step {step.GetType().Name}...");
                step.ToMachineReadable(model);
            }
            model.WriteTo(new FolderFilesStore(tempFolderPath), false);

            logger.LogDebug($"Zipping temp output folder...");
            // create zip from output folder
            if (File.Exists(pbitFile))
                File.Delete(pbitFile);
            ZipFile.CreateFromDirectory(tempFolderPath, pbitFile);

            logger.LogDebug($"Removing temp output folder...");
            Directory.Delete(tempFolderPath, true);

            logger.LogInformation($"Packing complete!");

        }
    }
}
