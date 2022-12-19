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
            var transform = new MinifyExpressionsLayoutJsonTransform(logger);
            transform.Transform(obj);

            logger.LogInformation("Creting glossary of columns and measures");
            var glosaryStr = JsonConvert.SerializeObject(transform.Glossary, Formatting.Indented);
            fileSystem.Save("_glossary.json", glosaryStr);
        }

        public void Restore(JObject obj)
        {
            Lazy<ColumnsAndMeasuresGlossary> glossaryLazy = new Lazy<ColumnsAndMeasuresGlossary>(() =>
            {
                logger.LogInformation("Reading glossary of columns and measures");
                var glossaryStr = fileSystem.ReadAsString("_glossary.json");
                var glossary = JsonConvert.DeserializeObject<ColumnsAndMeasuresGlossary>(glossaryStr)!;
                return glossary;
            });

            var transform = new UnminifyExpressionsLayoutJsonTransform(new GlossaryDbInfoGetter(glossaryLazy), logger);
            transform.Transform(obj);
        }

        class GlossaryDbInfoGetter : IDbInfoGetter
        {
            private readonly Lazy<ColumnsAndMeasuresGlossary> glossaryLazy;

            public GlossaryDbInfoGetter(Lazy<ColumnsAndMeasuresGlossary> glossaryLazy)
            {
                this.glossaryLazy = glossaryLazy;
            }

            public bool IsColumn(string tableName, string propertyName)
            {
                return glossaryLazy.Value.IsColumn(tableName, propertyName);
            }

            public bool IsMeasure(string tableName, string propertyName)
            {
                return glossaryLazy.Value.IsMeasure(tableName, propertyName);
            }
        }
    }
}
