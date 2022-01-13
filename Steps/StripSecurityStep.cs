using Packer.Model;
using System.Xml.Linq;

namespace Packer.Steps
{
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
