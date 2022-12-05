using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Packer2.FileSystem;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Transofrmations;

namespace Packer2.Library
{
    public static class TabularModel
    {
        public static Database LoadFromPbitFile(string path)
        {
            var dataModelSchemaFile = new PBIArchiveStore(path).Read().DataModelSchemaFile;
            return new BimDataModelStore(new JObjFile(dataModelSchemaFile)).Read();
        }

        public static Database LoadFromBimFile(string path)
            => new BimDataModelStore(new TextFileStore(path)).Read();

        public static Database LoadFromFolder(string path)
            => new FolderDatabaseStore(new LocalFileSystem(path)).Read();

        public static Database LoadFromSSAS(string connectionString)
            => new SSASDataModelStore(connectionString).Read();

        public static Database LoadFromSSAS(string server, string database)
            => new SSASDataModelStore(server, database).Read();

        public static Database ExtractDataSourcesOnly(this Database db)
            => new ExportDataSourcesTransform().Transform(db);

        public static Database DeclareDataSources(this Database db)
            => new RegisterDataSourcesTransform().Transform(db);

        public static Database SetCompatibilityLimit(this Database db, int compatibilityLevel)
            => new DowngradeTransform(compatibilityLevel).Transform(db);

        public static Database PullUpExpressions(this Database db)
            => new PullUpExpressionsTranform().Transform(db);
        
        public static Database StripCultures(this Database db)
            => new StripCulturesTransform().Transform(db);

        public static Database StripAutoDateTables(this Database db)
            => new StripLocalDateTablesTransform().Transform(db);


        public static void SaveToFolder(this Database db, string folderPath)
            => new FolderDatabaseStore(folderPath).Save(db);

        public static void SaveToSSAS(this Database db, string server, string database)
            => new SSASDataModelStore(server, database).Save(db);

        public static void SaveToBimFile(this Database db, string filePath)
            => new BimDataModelStore(new TextFileStore(filePath)).Save(db);
    }
}
