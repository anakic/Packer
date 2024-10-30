using Newtonsoft.Json.Linq;
using Packer2.FileSystem;

namespace Packer2.Library.Tools
{
    // todo: add logging to this class and all zone-mapping base classes
    abstract class MappingZone
    {
        PathEscaper pathEscaper = new PathEscaper();

        // root folder for all items of this type
        protected abstract string ContainingFolder { get; }

        // how to find child elements in the parent jobj
        protected abstract string ElementsSelector { get; }

        protected virtual bool FilterSelectedElements(JToken element) => true;

        protected virtual bool FilterSelectedProcessedElements(JToken element) => true;

        // which child elements are we mapping (recursively)
        protected abstract IEnumerable<MappingZone> ChildMappings { get; }
        
        // how shall we name a sub-sub-folder for each element (the complete relative path is [ZoneFolder]/[ElementFolder], e.g. Tables/Employees)
        protected abstract string GetSubfolderForElement(JToken elem);

        // how shall we name the file for an element
        protected abstract string GetFileName(JToken elem);

        // which extension will the file for a given element have
        protected abstract string GetFileExtension(JToken elem);

        // what payload should we store from the element (JToken) into the file
        public abstract string GetPayload(JToken obj);

        // change the element so that it only has the payload location
        protected abstract void RegisterPayloadLocation(JToken elem, string destinationFileName);

        protected abstract JToken ApplyElementPayload(JToken elem, string payload);

        protected abstract string ReadPayloadLocation(JToken elem);

        public void Read(JToken obj, IFileSystem fileSystem, string relativeFolder)
        {
            var elements = obj.SelectTokens(ElementsSelector).Where(FilterSelectedProcessedElements);
            foreach (var elem in elements.ToArray())
            {
                var element = elem;

                // read the stored payload for this element
                var payloadLocation = ReadPayloadLocation(elem);

                if (payloadLocation == null)
                {
                    string errorMessage = "Invalid JSON in element for model.\nObject path: " + relativeFolder + "\nElement:" + element;
                    throw new InvalidDataException(errorMessage);
                }

                // read 
                var payload = fileSystem.ReadAsString(Path.Combine(relativeFolder, payloadLocation));

                // then apply the payload to the element (might replace the element, which is why it return an element)
                element = ApplyElementPayload(elem, payload);

                // which folder does this element store data in (so we can pass this to child zones)
                var subFolderForElement = Path.Combine(relativeFolder, ContainingFolder, pathEscaper.EscapeName(GetSubfolderForElement(element)));

                // read child elements first
                foreach (var childMap in ChildMappings)
                    childMap.Read(element, fileSystem, subFolderForElement);
            }
        }

        public void Write(JToken obj, IFileSystem fileSystem, string relativeFolder)
        {
            var elements = obj.SelectTokens(ElementsSelector).Where(FilterSelectedElements);
            foreach (var elem in elements.ToArray())
            {
                // where shall we store the data from this element
                var subFolderForElementRelativeToParent = Path.Combine(ContainingFolder, pathEscaper.EscapeName(GetSubfolderForElement(elem)));
                var subFolderForElementRelativeToBase = Path.Combine(relativeFolder, subFolderForElementRelativeToParent);
                
                // process child mappings first (while everything is in the same file)
                foreach (var childMap in ChildMappings)
                    childMap.Write(elem, fileSystem, subFolderForElementRelativeToBase);

                // then extract the payload from the JToken
                var payload = GetPayload(elem);

                // store the payload into a file
                var destinationFileNameRelativeToParent = Path.Combine(subFolderForElementRelativeToParent, $"{pathEscaper.EscapeName(GetFileName(elem))}.{GetFileExtension(elem)}" );
                fileSystem.Save(Path.Combine(relativeFolder, destinationFileNameRelativeToParent), payload);

                // register the location of the payload in the JToken
                RegisterPayloadLocation(elem, destinationFileNameRelativeToParent);
            }
        }
    }
}
