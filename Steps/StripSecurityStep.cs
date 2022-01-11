using System.Xml.Linq;

namespace Packer.Steps
{
    internal class StripSecurityStep : StepBase
    {
        public override void Extract(string pbitFilePath, string folderPath)
        {
            // remove security bindings
            File.Delete(Path.Combine(folderPath, "SecurityBindings"));
            var contentTypesXmlPath = Path.Combine(folderPath, "[Content_Types].xml");
            var doc = XDocument.Load(contentTypesXmlPath);
            doc
                .Descendants(XName.Get("Override", @"http://schemas.openxmlformats.org/package/2006/content-types"))
                .SingleOrDefault(xe => xe.Attribute("PartName")?.Value == "/SecurityBindings")
                ?.Remove();
            doc.Save(contentTypesXmlPath);

            base.Extract(pbitFilePath, folderPath);
        }
    }
}
