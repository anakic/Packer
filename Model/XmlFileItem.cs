using Packer.Storage;
using System.Text;
using System.Xml.Linq;

namespace Packer.Model
{
    /// <summary>
    /// Represents an XML file the repository. Exposes an XDocument
    /// property that can be get/set and modified. 
    /// </summary>
    /// <remarks>
    /// Currently only used for the [Content_Types].xml file</remarks>
    class XmlFileItem : FileSystemItem
    {
        public XmlFileItem(string path, XDocument xDocument) 
            : base(path)
        {
            XDocument = xDocument;
        }

        public XDocument XDocument { get; }

        internal override void SaveForMachine(IFilesStore store)
            => SaveForHuman(store);

        internal override void SaveForHuman(IFilesStore store)
        {
            store.Write(Path, XDocument.ToString(), Encoding.UTF8);
        }
    }
}
