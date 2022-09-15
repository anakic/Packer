using Microsoft.Extensions.Logging;
using Packer.Model;
using Packer.Steps;
using Packer.Storage;
using Packer.TMP;
using System.IO.Compression;
using static System.Formats.Asn1.AsnWriter;

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
        List<StepBase> mainRepoSteps = new List<StepBase>();
        List<StepBase> modelRepoSteps = new List<StepBase>();

        public Engine(ILoggerFactory loggerFactory)
        {
            mainRepoSteps.Add(new StripSecurityStep());
            mainRepoSteps.Add(new StripTimestapsStep());
            mainRepoSteps.Add(new ExtractTablesStep());
            mainRepoSteps.Add(new ExtractDaxStep());
            mainRepoSteps.Add(new ExtractMStep());
            mainRepoSteps.Add(new ExtractPagesStep());
            mainRepoSteps.Add(new ConsolidateVisualsOrderingStep());
            mainRepoSteps.Add(new OrderArraysStep());
            mainRepoSteps.Add(new StripPageGenPropsSteps());
            mainRepoSteps.Add(new UnstuffJsonStep());
            mainRepoSteps.Add(new ExtractBookmarksStep()); // must be after unstuff because bookmarks are stuffed inside layout/config property
            mainRepoSteps.Add(new SetSchemasStep(loggerFactory.CreateLogger<SetSchemasStep>()));
            mainRepoSteps.Add(new ResolveVariablesStep());


            logger = loggerFactory.CreateLogger<Engine>();
        }

        public void MigrateToSSAS(string sourceBimFilePath, string outputBimFilePath)
        {
            var store = new BimModelStore(sourceBimFilePath);
            var outStore = new BimModelStore(outputBimFilePath);
            var transform = new ModelSassAdapter();

            var model = store.Read();
            transform.Transform(model);
            outStore.Save(model);
        }

        public void Extract(string pbitFile, string repositoryFolder)
        {
            logger.LogInformation($"Extracting file '{pbitFile}' to folder '{repositoryFolder}'");
            var folderStore = new FolderFilesStore(repositoryFolder);

            logger.LogDebug($"Unzipping...");
            folderStore.UpdateContentsFromZip(pbitFile);

            var repoModel = new RepositoryModel(folderStore);
            foreach (var step in mainRepoSteps)
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
            for (int i = mainRepoSteps.Count - 1; i >= 0; i--)
            {
                var step = mainRepoSteps[i];
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
