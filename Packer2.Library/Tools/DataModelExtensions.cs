using Microsoft.AnalysisServices.Tabular;

namespace Packer2.Library.Tools
{
    static class DataModelExtensions
    {
        public static void RemoveFromModel(this Table table)
        {
            var model = table.Model;
            model.Tables.Remove(table);
            foreach (var r in model.Relationships.Where(t => t.ToTable == table || t.FromTable == table))
                model.Relationships.Remove(r);
        }

        public static void RemoveFromModel(this Column column)
        {
            var table = column.Table;
            var model = table.Model;
            table.Columns.Remove(column);

            // todo: might have to use reflection or serialize to json because the relatiohsip class does not expose From/To Column (it's internal for some reason)
            foreach (var r in model.Relationships.Where(t => t. == table || t.FromTable == table))
                model.Relationships.Remove(r);
        }
    }
}
