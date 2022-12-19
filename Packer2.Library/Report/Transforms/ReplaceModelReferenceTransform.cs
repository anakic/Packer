using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Packer2.Library.Report.Transforms
{
    public record TableObjectMapping(string OldObjectName, string NewObjectName, string NewTableName);

    public class TableMappings
    {
        Dictionary<string, TableObjectMapping> objectMoves = new Dictionary<string, TableObjectMapping>();
        public IEnumerable<TableObjectMapping> ObjectMoves => objectMoves.Values;

        public TableMappings(string tableName)
        {
            TableName = tableName;
            NewTableName = tableName;
        }

        public TableMappings MapTo(string newName)
        {
            NewTableName = newName;
            return this;
        }

        public TableMappings MapObjectTo(string name, string newName, string? newParentTable = null)
        {
            objectMoves.Add(name, new TableObjectMapping(name, newName, newParentTable ?? NewTableName));
            return this;
        }

        public bool TryGetMapping(string originalName, [MaybeNullWhen(false)] out TableObjectMapping change)
            => objectMoves.TryGetValue(originalName, out change);

        public string TableName { get; }
        public string NewTableName { get; private set; }
    }

    public class Mappings
    {
        Dictionary<string, TableMappings> tableMappings = new Dictionary<string, TableMappings>();
        public TableMappings Table(string name)
        {
            if (tableMappings.TryGetValue(name, out var trc))
                return trc;
            else
                return tableMappings[name] = new TableMappings(name);
        }

        public bool TryGetNewTableName(string originalName, [MaybeNullWhen(false)] out string newTableName)
        {
            if (tableMappings.TryGetValue(originalName, out var tableObjMappings))
            {
                if (tableObjMappings.NewTableName != tableObjMappings.TableName)
                {
                    newTableName = tableObjMappings.NewTableName;
                    return true;
                }
            }

            newTableName = null;
            return false;
        }

        public bool TryGetMappedObjectInfo(string originalTableName, string originalObjectName, [MaybeNullWhen(false)] out TableObjectMapping objectMapping)
        {
            if (tableMappings.TryGetValue(originalTableName, out var tableObjMappings))
            {
                if (tableObjMappings.TryGetMapping(originalObjectName, out objectMapping))
                {
                    return true;
                }
                // if there are no direct mappings for object on the original table, then it will move to the mapped-to table
                // so we need to check if that table has mappings for it
                else if(tableMappings.TryGetValue(tableObjMappings.NewTableName, out var mappedToTableObjMappings))
                {
                    if (mappedToTableObjMappings.TryGetMapping(originalObjectName, out objectMapping))
                    {
                        return true;
                    }
                }
            }

            objectMapping = default;
            return false;
        }

        // this is a very non-exact way of trying to figure out if 
        public IEnumerable<string> GetTablesMappedTo(string key)
        {
            return tableMappings.Where(tm => tm.Value.NewTableName == key).Select(x => x.Key);
        }
    }

    public class ReplaceModelReferenceTransform : ReportInfoNavTransformBase
    {
        private readonly Mappings mappings;

        public ReplaceModelReferenceTransform(Mappings renames, ILogger? logger = null)
            : base(logger)
        {
            this.mappings = renames;
        }

        protected override void Process(FilterDefinition filter, string path)
        {
            filter.MapReferences(mappings);
        }
        protected override void Process(QueryDefinition query, string path)
        {
            query.MapReferences(mappings);
        }
        protected override void Process(QueryExpressionContainer expression, string path)
        {
            expression.MapReferences(mappings);
        }
    }
}
