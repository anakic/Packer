using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.Tools;
using System.Linq;

namespace Packer2.Library.DataModel.Customizations
{
    internal class DataModelFilter
    {
        private readonly List<TableRuleFilter> tableFilters;
        private readonly List<TableObjectRuleFilter> objectFilters;

        public DataModelFilter(IEnumerable<IgnoreRule> rules)
        {
            tableFilters = rules
                .Where(r => r.ObjectPattern == null)
                .Select(r => new TableRuleFilter
                (
                    Rule: r,
                    TableFilter: new NameFilter(r.TablePattern)
                ))
                .ToList();

            objectFilters = rules
                .Where(r => r.ObjectPattern != null)
                .Select(r => new TableObjectRuleFilter
                (
                    Rule: r,
                    TableFilter: new NameFilter(r.TablePattern),
                    ObjectFilter: new NameFilter(r.ObjectPattern!)
                ))
                .ToList();
        }

        public void Extend(Database targetDatabase, Database fullDatabase)
        {
            List<Table> copiedTables = new List<Table>();
            List<Column> copiedColumns = new List<Column>();
            foreach (var table in fullDatabase.Model.Tables.ToArray())
            {
                var shouldKeepTable = ShouldKeepTable(table.Name, tableFilters);
                if (!shouldKeepTable)
                {
                    targetDatabase.Model.Tables.Add(table.Clone());
                    copiedTables.Add(table);
                    continue;
                }

                foreach (var c in table.Columns.ToArray())
                {
                    var shouldKeepObject = ShouldKeepObject(c.Table.Name, c.Name, objectFilters);
                    if (!shouldKeepObject)
                    {
                        targetDatabase.Model.Tables[table.Name].Columns.Add(c.Clone());
                        copiedColumns.Add(c);
                    }
                }

                foreach (var m in table.Measures.ToArray())
                {
                    var shouldKeepObject = ShouldKeepObject(m.Table.Name, m.Name, objectFilters);
                    if (!shouldKeepObject)
                        targetDatabase.Model.Tables[table.Name].Measures.Add(m.Clone());
                }

                foreach (var h in table.Hierarchies.ToArray())
                {
                    var shouldKeepObject = ShouldKeepObject(h.Table.Name, h.Name, objectFilters);
                    if (!shouldKeepObject)
                        targetDatabase.Model.Tables[table.Name].Hierarchies.Add(h.Clone());
                }
            }

            var relationshipsToAdd =
                fullDatabase.Model.Relationships.Where(r =>
                {
                    return copiedTables.Contains(r.FromTable)
                        || copiedTables.Contains(r.ToTable)
                        || copiedColumns.Contains(r.GetFromColumn())
                        || copiedColumns.Contains(r.GetToColumn());
                })
                .ToArray();

            foreach(var r in relationshipsToAdd)
                targetDatabase.Model.Relationships.Add(r.Clone());
        }

        public void Crop(Database database)
        {
            foreach (var table in database.Model.Tables.ToArray())
            {
                var shouldKeepTable = ShouldKeepTable(table.Name, tableFilters);
                if (!shouldKeepTable)
                {
                    table.RemoveFromModel();
                    continue;
                }

                foreach (var c in table.Columns.ToArray())
                {
                    var shouldKeepObject = ShouldKeepObject(c.Table.Name, c.Name, objectFilters);
                    if (!shouldKeepObject)
                        c.RemoveFromModel();
                }

                foreach (var m in table.Measures.ToArray())
                {
                    var shouldKeepObject = ShouldKeepObject(m.Table.Name, m.Name, objectFilters);
                    if (!shouldKeepObject)
                        table.Measures.Remove(m);
                }

                foreach (var h in table.Hierarchies.ToArray())
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
                    shouldRemove = f.Rule.Action == TargetAction.Ignore;
            }
            return shouldRemove;
        }

        private bool ShouldKeepTable(string tableName, List<TableRuleFilter> tableFilters)
        {
            bool shouldRemove = true;
            foreach (var f in tableFilters)
            {
                if (f.TableFilter.IsMatch(tableName))
                    shouldRemove = f.Rule.Action == TargetAction.Ignore;
            }
            return shouldRemove;
        }

        record TableRuleFilter(IgnoreRule Rule, NameFilter TableFilter);
        record TableObjectRuleFilter(IgnoreRule Rule, NameFilter TableFilter, NameFilter ObjectFilter);
    }
}
