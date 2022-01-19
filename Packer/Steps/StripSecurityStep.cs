using Packer.Model;
using System.Xml.Linq;

namespace Packer.Steps
{
    /// <summary>
    /// Strips the security bindings from the repo allowing us to edit the code and repackage the pbit file.
    /// If this is not done, PowerBI will complain about the file being corrupt when openning the pbit file.
    /// </summary>
    internal class StripSecurityStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            // remove security bindings
            model.SecurityBindings = null;
            // update xml file (remove sec binding entry)
            model.ContentTypesFile!.XDocument
                .Descendants(XName.Get("Override", @"http://schemas.openxmlformats.org/package/2006/content-types"))
                .SingleOrDefault(xe => xe.Attribute("PartName")?.Value == "/SecurityBindings")
                ?.Remove();

            base.ToHumanReadable(model);
        }
    }
}
