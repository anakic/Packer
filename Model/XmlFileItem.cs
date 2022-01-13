using Packer.Storage;
using System.Text;
using System.Xml.Linq;

namespace Packer.Model
{
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
