using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel;
using Packer2.Library.DataModel.Transofrmations;

namespace Packer2.Library
{
    public static class Packer
    {
        public static Database ReadDataModelFromBimFile(string path)
            => new BimDataModelStore(new LocalTextFile(path)).Read();

        public static Database DeclareDataSources(this Database db)
            => new RegisterDataSourcesTransform().Transform(db);

        public static Database DowngradeTo(this Database db, int compatibilityLevel)
            => new DowngradeTransform(compatibilityLevel).Transform(db);

        public static Database PullUpExpressions(this Database db)
            => new PullUpExpressionsTranform().Transform(db);
        
        public static Database StripCultures(this Database db)
            => new StripCulturesTransform().Transform(db);

        public static Database StripAutoDateTables(this Database db)
            => new StripLocalDateTablesTransform().Transform(db);

        public static void SaveToRepo(this Database db, string folderPath)
            => new FolderModelStore(folderPath).Save(db);

        public static void SaveToFile(this Database db, string filePath)
            => new BimDataModelStore(new LocalTextFile(filePath)).Save(db);
    }
}
