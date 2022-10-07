using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel.Transofrmations
{
    public class StripLocalDateTablesTransform : IDataModelTransform
    {
        private readonly ILogger<StripLocalDateTablesTransform> logger;

        public StripLocalDateTablesTransform(ILogger<StripLocalDateTablesTransform>? logger = null)
        {
            this.logger = logger ?? new DummyLogger<StripLocalDateTablesTransform>();
        }

        public Database Transform(Database database)
        {
            var tables = database.Model.Tables.Where(t => t.IsHidden && t.ShowAsVariationsOnly && t.Name.StartsWith("LocalDateTable_")).ToList();

            var variationsByRelation = database.Model.Tables.SelectMany(t => t.Columns).SelectMany(c => c.Variations).GroupBy(v => v.Relationship).ToDictionary(g => g.Key, g => g);

            foreach (var tn in tables)
            {
                logger.LogInformation("Removing auto time-intelligence table {tableName}", tn);
                database.Model.Relationships.Where(r => r.ToTable == tn).ToList().ForEach(r =>
                {
                    // remove variations for this relation
                    variationsByRelation[r].ToList().ForEach(v => { v.Column.Variations.Remove(v); logger.LogInformation("Removing variation {variation}", v.Name); });
                    // remove the relation
                    logger.LogInformation("Removing relationship {relationship}", r.Name);
                    database.Model.Relationships.Remove(r);
                });
                // remove the date table
                database.Model.Tables.Remove(tn);
            }

            // todo: we could replace variation use in dax expressions (antlr)

            return database;
        }
    }
}
