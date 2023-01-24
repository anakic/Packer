using Microsoft.AnalysisServices.Tabular;
using System.Reflection;

namespace Packer2.Library.Tools
{
    static class DataModelExtensions
    {
        public static void RemoveFromModel(this Table table)
        {
            var model = table.Model;
            model.Tables.Remove(table);
            foreach (var r in model.Relationships.Where(t => t.ToTable == table || t.FromTable == table).ToArray())
                model.Relationships.Remove(r);
        }

        // todo: have to use reflection because the relatiohsip class does not expose From/To Column (it's internal for some reason)
        // (alternative would be to serialize to json)
        public static Column GetFromColumn(this Relationship relationship)
            => (Column)typeof(Relationship).GetProperty("FromColumn", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(relationship)!;

        // todo: have to use reflection because the relatiohsip class does not expose From/To Column (it's internal for some reason)
        // (alternative would be to serialize to json)
        public static Column GetToColumn(this Relationship relationship)
            => (Column)typeof(Relationship).GetProperty("ToColumn", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(relationship)!;

        public static void RemoveFromModel(this Column column)
        {
            var table = column.Table;
            var model = table.Model;
            table.Columns.Remove(column);

            foreach (var r in model.Relationships.Where(t => (t.GetFromColumn() == column) || t.GetToColumn() == column).ToArray())
                model.Relationships.Remove(r);
        }
    }
}
