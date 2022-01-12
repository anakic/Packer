using System.Xml.Linq;

namespace Packer.Model
{
    class XmlFileItem : FileItem
    {
        Lazy<XDocument> lazyDocument;

        public XmlFileItem(string basePath, string relativePath) : base(basePath, relativePath)
        {
            lazyDocument = new Lazy<XDocument>(() => XDocument.Parse(ReadAsString()));
        }

        public void Modify(Action<XDocument> documentAction)
        {
            documentAction(lazyDocument.Value);
            lazyDocument.Value.Save(AbsolutePath);
        }
    }
}
