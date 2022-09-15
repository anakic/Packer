using Packer.Model;

namespace Packer.TMP
{
    public class ReportModel
    {
        IEnumerable<FileSystemItem> Files { get; }

        // todo: expose properties for specific areas of the model
        // each model must have: connections file, settings file, version file, pages folder, version file, optional model file or folder

        public ReportModel(IEnumerable<FileSystemItem> files)
        {
            Files = files;
        }
    }
}
