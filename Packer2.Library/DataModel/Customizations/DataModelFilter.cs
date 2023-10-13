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
            // todo: we currently only check table and their nested objects (columns, measures, hierarchies)
            // but if we want to rename an expression, the old one won't get deleted. To fix this, we need to 
            // include folders e.g. Tables/ and Expressions/ to filter patterns and check for expressions as well
            // in the Extend and Crop methods

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
            foreach (var expr in fullDatabase.Model.Expressions.ToArray())
            {
                var shouldKeepExpression = ShouldKeepObject(expr.Name, tableFilters);
                if (!shouldKeepExpression)
                    targetDatabase.Model.Expressions.Add(expr.Clone());
            }

            List<Table> copiedTables = new List<Table>();
            List<Column> copiedColumns = new List<Column>();
            foreach (var table in fullDatabase.Model.Tables.ToArray())
            {
                var shouldKeepTable = ShouldKeepObject(table.Name, tableFilters);
                if (!shouldKeepTable)
                {
                    // See comment on the Clone(table) helper method below
                    // for exmplanation why we're not using table.Clone() here.
                    targetDatabase.Model.Tables.Add(Clone(table));
                    copiedTables.Add(table);
                    continue;
                }

                foreach (var c in table.Columns.ToArray())
                {
                    var shouldKeepObject = ShouldKeepNestedObject(c.Table.Name, c.Name, objectFilters);
                    if (!shouldKeepObject)
                    {
                        targetDatabase.Model.Tables[table.Name].Columns.Add(c.Clone());
                        copiedColumns.Add(c);
                    }
                }

                foreach (var m in table.Measures.ToArray())
                {
                    var shouldKeepObject = ShouldKeepNestedObject(m.Table.Name, m.Name, objectFilters);
                    if (!shouldKeepObject)
                        targetDatabase.Model.Tables[table.Name].Measures.Add(m.Clone());
                }

                foreach (var h in table.Hierarchies.ToArray())
                {
                    var shouldKeepObject = ShouldKeepNestedObject(h.Table.Name, h.Name, objectFilters);
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
            foreach (var expr in database.Model.Expressions.ToArray())
            {
                var shouldKeepTable = ShouldKeepObject(expr.Name, tableFilters);
                if (!shouldKeepTable)
                    database.Model.Expressions.Remove(expr);
            }

            foreach (var table in database.Model.Tables.ToArray())
            {
                var shouldKeepTable = ShouldKeepObject(table.Name, tableFilters);
                if (!shouldKeepTable)
                {
                    table.RemoveFromModel();
                    continue;
                }

                foreach (var c in table.Columns.ToArray())
                {
                    var shouldKeepObject = ShouldKeepNestedObject(c.Table.Name, c.Name, objectFilters);
                    if (!shouldKeepObject)
                        c.RemoveFromModel();
                }

                foreach (var m in table.Measures.ToArray())
                {
                    var shouldKeepObject = ShouldKeepNestedObject(m.Table.Name, m.Name, objectFilters);
                    if (!shouldKeepObject)
                        table.Measures.Remove(m);
                }

                foreach (var h in table.Hierarchies.ToArray())
                {
                    var shouldKeepObject = ShouldKeepNestedObject(h.Table.Name, h.Name, objectFilters);
                    if (!shouldKeepObject)
                        table.Hierarchies.Remove(h);
                }
            }
        }

        // The purpose of this method is to clone tables. The built-in clone method does not preserve calculated column
        // data types. Example of problem: if we read a model that does not include a table which contains calculated columns,
        // (i.e. the table is not part of the customization), when we try to save the model, calculated columns on that table 
        // will have their data types reset to "Unknown". This happens because before saving the model, we augment it with 
        // the cropped out tables, but when the table is read from the full (uncropped) database and then cloned
        // using the table.Clone() method, it loses its datatype (set to Unknown). To avoid this, instead of using table.Clone()
        // we use the JsonSerializer to clone the table. Currently only using for tables because only table.Clone() seems to cause issues.
        // If needed, can also be used for columns, measures, relationships, expressions, etc...
        private T Clone<T>(T obj) where T : MetadataObject
        {
            return JsonSerializer.DeserializeObject<T>(JsonSerializer.SerializeObject(obj));
        }

        private bool ShouldKeepNestedObject(string tableName, string objectName, List<TableObjectRuleFilter> objectFilters)
        {
            bool shouldRemove = true;
            foreach (var f in objectFilters)
            {
                if (f.TableFilter.IsMatch(tableName) && f.ObjectFilter.IsMatch(objectName))
                    shouldRemove = f.Rule.Action == TargetAction.Ignore;
            }
            return shouldRemove;
        }

        private bool ShouldKeepObject(string tableName, List<TableRuleFilter> tableFilters)
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
