using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Packer2.FileSystem;
using Packer2.Library.MinifiedQueryParser.QueryTransforms;

namespace Packer2.Library.Report.Stores.Folder.Transforms
{
    class MinifyQueriesTransform : IJObjTransform
    {
        private readonly IFileSystem fileSystem;
        private readonly ILogger logger;

        public MinifyQueriesTransform(IFileSystem fileSystem, ILogger logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
        }

        public void Transform(JObject obj)
        {
            var transform = new MinifyExpressionsLayoutJsonTransform();
            transform.Transform(obj);

            var glosaryStr = JsonConvert.SerializeObject(transform.Glossary);
            fileSystem.Save("_glossary.json", glosaryStr);
        }

        public void Restore(JObject obj)
        {
            var glosaryStr = fileSystem.ReadAsString("_glossary.json");
            var glossary = JsonConvert.DeserializeObject<ColumnsAndMeasuresGlossary>(glosaryStr);

            var transform = new UnminifyExpressionsLayoutJsonTransform(glossary, logger);
            transform.Transform(obj);
        }
    }
}
