using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.Tools;

namespace Packer2.Library.DataModel.Customizations
{
    internal class DataModelFilter
    {
        private readonly IEnumerable<IgnoreRule> rules;

        public DataModelFilter(IEnumerable<IgnoreRule> rules)
        {
            this.rules = rules;
        }

        public void Apply(Database database)
        {
            var tableFilters = rules
                .Where(r => r.ObjectPattern == null)
                .Select(r => new TableRuleFilter
                (
                    Rule: r,
                    TableFilter: new NameFilter(r.TablePattern)
                ))
                .ToList();

            var objectFilters = rules
                .Where(r => r.ObjectPattern != null)
                .Select(r => new TableObjectRuleFilter
                (
                    Rule: r,
                    TableFilter: new NameFilter(r.TablePattern),
                    ObjectFilter: new NameFilter(r.ObjectPattern!)
                ))
                .ToList();

            foreach (var table in database.Model.Tables)
            {
                var shouldKeepTable = ShouldKeepTable(table.Name, tableFilters);
                if (!shouldKeepTable)
                {
                    table.RemoveFromModel();
                    continue;
                }

                foreach (var c in table.Columns)
                {
                    var shouldKeepObject = ShouldKeepObject(c.Table.Name, c.Name, objectFilters);
                    if (!shouldKeepObject)
                        c.RemoveFromModel();
                }

                foreach (var m in table.Measures)
                {
                    var shouldKeepObject = ShouldKeepObject(m.Table.Name, m.Name, objectFilters);
                    if (!shouldKeepObject)
                        table.Measures.Remove(m);
                }

                foreach (var h in table.Hierarchies)
                {
                    var shouldKeepObject = ShouldKeepObject(h.Table.Name, h.Name, objectFilters);
                    if (!shouldKeepObject)
                        table.Hierarchies.Remove(h);
                }
            }
        }

        private bool ShouldKeepObject(string tableName, string objectName, List<TableObjectRuleFilter> objectFilters)
        {
            bool shouldRemove = true;
            foreach (var f in objectFilters)
            {
                if (f.TableFilter.IsMatch(tableName) && f.ObjectFilter.IsMatch(objectName))
                    shouldRemove = f.Rule.Invert;
            }
            return shouldRemove;
        }

        private bool ShouldKeepTable(string tableName, List<TableRuleFilter> tableFilters)
        {
            bool shouldRemove = true;
            foreach (var f in tableFilters)
            {
                if(f.TableFilter.IsMatch(tableName))
                    shouldRemove = f.Rule.Invert;
            }
            return shouldRemove;
        }

        record TableRuleFilter(IgnoreRule Rule, NameFilter TableFilter);
        record TableObjectRuleFilter(IgnoreRule Rule, NameFilter TableFilter, NameFilter ObjectFilter);
    }
}
