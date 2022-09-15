using Microsoft.AnalysisServices.Tabular;
using System.Linq;

namespace DataModelLoader.Transofrmations
{
    public class StripLocalDateTablesTransform : IDataModelTransform
    {
        public Database Transform(Database database)
        {
            var tables = database.Model.Tables.Where(t => t.IsHidden && t.ShowAsVariationsOnly && t.Name.StartsWith("LocalDateTable_")).ToList();

            var variationsByRelation = database.Model.Tables.SelectMany(t => t.Columns).SelectMany(c => c.Variations).GroupBy(v => v.Relationship).ToDictionary(g => g.Key, g => g);

            foreach (var tn in tables)
            {
                database.Model.Relationships.Where(r => r.ToTable == tn).ToList().ForEach(r => 
                {
                    // remove variations for this relation
                    variationsByRelation[r].ToList().ForEach(v => v.Column.Variations.Remove(v));
                    // remove the relation
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
