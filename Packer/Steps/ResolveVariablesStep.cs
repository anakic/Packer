using Newtonsoft.Json.Linq;
using Packer.Model;

namespace Packer.Steps
{
    // todo: consider removing this step as well as $fileRef and $fileStringRef. We can use
    // the folder structure and file names to construct a bim/datamodelschema file from a folder

    /// <summary>
    /// This step runs only when converting to machine readable format. It replaces all $fileRef 
    /// elements created by the Extract[Tables/Pages]Step with the contents of those files. This 
    /// makes it so that extract steps do not have to pack themselves. It is important that this
    /// step is added AFTER the extract steps, meaning that it executes before (when packing) so
    /// that the extract steps can just clean up the extracted files (because this step would have
    /// already executed.
    /// </summary>
    internal class ResolveVariablesStep : StepBase
    {
        public override void ToMachineReadable(RepositoryModel model)
        {
            base.ToMachineReadable(model);

            model.GetAllJsonFiles()
                .ToList()
                .ForEach(jf =>
                {
                    // handle $fileRef (replace a child object with a reference to a json file, used for extracting tables/pages into own files)
                    var fileRefObjects = jf.JObj.Descendants().OfType<JObject>()
                        .Select(obj => new { obj, fileRef = obj.Property("$fileRef")?.Value.ToString() })
                        .Where(x => x.fileRef != null)
                        .ToList();
                    foreach (var fileRefObj in fileRefObjects)
                    {
                        var resolved = model.GetExtractedJsonFile(fileRefObj.fileRef!);
                        fileRefObj.obj.Replace(resolved.JObj);
                    }

                    // handle $fileStringRef (replace a string with a ref to a text file with the contents of the string, used to extract m/dax code)
                    var fileStringRefObjects = jf.JObj.Descendants().OfType<JObject>()
                        .Select(obj => new { obj, fileRef = obj.Property("$fileStringRef")?.Value.ToString() })
                        .Where(x => x.fileRef != null)
                        .ToList();
                    foreach (var fileRefObj in fileStringRefObjects)
                    {
                        var resolved = model.GetExtractedTextFile(fileRefObj.fileRef!);
                        fileRefObj.obj.Replace(new JValue(resolved.Text));
                    }
                });
        }
    }
}
