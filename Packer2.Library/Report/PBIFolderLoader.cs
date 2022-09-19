using Newtonsoft.Json;
using Packer2.Library;

namespace DataModelLoader.Report
{
    public class PBIFolderLoader : IModelStore<PowerBIReport>
    {
        private readonly string folderPath;

        public PBIFolderLoader(string folderPath)
        {
            this.folderPath = folderPath;
        }

        public PowerBIReport Read()
        {
            throw new NotImplementedException();
        }

        private void ClearFolder()
        {
            if (Directory.Exists(folderPath))
            {
                foreach (var childDir in Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly))
                {
                    // do not remove the .git folder
                    if (Path.GetFileName(childDir) != ".git")
                        Directory.Delete(childDir, true);
                }

                foreach (var file in Directory.GetFiles(folderPath))
                    File.Delete(file);
            }
        }


        public void Save(PowerBIReport model)
        {
            ClearFolder();

            foreach (var kvp in model.Blobs)
            {
                var path = Path.Combine(folderPath, kvp.Key);
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllBytes(path, kvp.Value);
            }

            // todo: define or reuse constants for file names
            File.WriteAllText(Path.Combine(folderPath, "Connections.json"), model.Connections.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "[Content_Types].xml"), model.Content_Types.ToString());
            File.WriteAllText(Path.Combine(folderPath, "DataModelSchema.json"), model.DataModelSchemaFile.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "DiagramLayout.json"), model.DiagramLayout.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "Metadata.json"), model.Metadata.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "Settings.json"), model.Settings.ToString(Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "Report\\Layout.json"), JsonConvert.SerializeObject(model.Layout, Formatting.Indented));
            File.WriteAllText(Path.Combine(folderPath, "Report\\LinguisticSchema.xml"), model.Report_LinguisticSchema.ToString());
        }
    }
}
