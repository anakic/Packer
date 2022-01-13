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

        internal override byte[] GetBytesToSave()
        {
            return Encoding.Unicode.GetBytes(XDocument.ToString());
        }
    }
}
