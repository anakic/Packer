using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.Report.Queries;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Transforms
{
    public static class QueryModelExtensions_Map
    {
        public static void MapReferences(this QueryDefinition queryDefinition, Mappings mappings)
        {
            var visitor = new ReplaceReferencesVisitor(mappings);
            visitor.Visit(queryDefinition);
        }

        public static void MapReferences(this FilterDefinition filterDefinition, Mappings mappings)
        {
            var visitor = new ReplaceReferencesVisitor(mappings);
            visitor.Visit(filterDefinition);
        }

        public static void MapReferences(this QueryExpression expression, Mappings mappings)
        {
            var visitor = new ReplaceReferencesVisitor(mappings);
            visitor.VisitExpression(expression);
        }

        public static void MapReferences(this QueryExpressionContainer expressionContainer, Mappings mappings)
        {
            MapReferences(expressionContainer.Expression, mappings);
        }

        class ReplaceReferencesVisitor : BaseTransformVisitor
        {
            private readonly Mappings renames;

            public ReplaceReferencesVisitor(Mappings renames)
            {
                this.renames = renames;
            }

            public override void Visit(FilterDefinition filterDefinition)
            {
                base.Visit(filterDefinition);
                ProcessFromSection(filterDefinition.From);
            }

            public override void Visit(QueryDefinition queryDefinition)
            {
                base.Visit(queryDefinition);
                ProcessFromSection(queryDefinition.From);
            }

            private void ProcessFromSection(List<EntitySource> fromSources)
            {
                // we're not touching the report-defined ones (they have a schema), but we must not overwrite their aliases either
                var takenAliases = fromSources.Where(x => x.Schema != null).Select(x => x.Name).ToHashSet();
                var sources = new List<EntitySource>();
                foreach (var kvp in aliasedPropertyExpressions)
                {
                    var alias = FindNextAvailableAlias(kvp.Key.Substring(0, 1).ToLower(), takenAliases);
                    takenAliases.Add(alias);
                    foreach (var exp in kvp.Value)
                    {
                        switch (exp)
                        {
                            case QueryPropertyExpression qpe:
                                qpe.Expression.SourceRef.Source = alias;
                                break;
                            case QueryHierarchyExpression qpe:
                                qpe.Expression.SourceRef.Source = alias;
                                break;
                            default:
                                throw new NotImplementedException("Currently only support moving/renaming measures, columns and hierarchies");
                        }
                    }
                    sources.Add(new EntitySource() { Entity = kvp.Key, Name = alias });
                }
                SyncSources(sources, fromSources);
            }

            private void SyncSources(List<EntitySource> sources, List<EntitySource> from)
            {
                var srcDict = sources.ToDictionary(s => s.Entity, f => f);
                // we don't want to mess with the ones that have a schema (they are report-defined measures, not in the model)
                var fromDict = from.Where(x => x.Schema == null && x.Entity != null).ToDictionary(f => f.Entity, f => f);
                foreach (var s in sources)
                {
                    // adjust alias for existing source if needed
                    if (fromDict.TryGetValue(s.Entity, out var fromEntity))
                        fromEntity.Name = s.Name;
                    else
                        from.Add(s);
                }

                foreach (var f in fromDict.Values)
                {
                    if (!srcDict.ContainsKey(f.Entity))
                        from.Remove(f);
                }
            }

            protected override BaseTransformVisitor CreateSubqueryVisitor()
            {
                return new ReplaceReferencesVisitor(renames);
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                Process(expression);
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                Process(expression);
            }

            protected override void Visit(QueryPropertyExpression expression)
            {
                Process(expression);
            }

            protected override void Visit(QuerySourceRefExpression expression)
            {
                // skipping over the ones that have schema set, those are extensions (report-defined measures)
                // todo: check if it's possible that it's from the model. If it is possible, we need to add support for model schemas
                if (expression.Schema != null)
                    return;

                // for expressions that aren't using aliases, just replace the table name (if needed)
                if (expression.Entity != null)
                {
                    var oldName = expression.Entity;
                    var newName = renames.Table(oldName).NewTableName;
                    if (newName != oldName)
                    {
                        expression.Entity = newName;
                    }
                }
            }

            Dictionary<string, List<QueryExpression>> aliasedPropertyExpressions = new Dictionary<string, List<QueryExpression>>();
            private void Process(QueryPropertyExpression expression)
            {
                base.Visit(expression);

                var sourceRef = expression.Expression.SourceRef;

                if (sourceRef == null)
                    return;

                string originalTableName;
                if (sourceRef.Source != null)
                {
                    var originalTableEntitySource = SourcesByAliasMap[sourceRef.Source];
                    // skipping over the ones that have schema set, those are extensions (report-defined measures)
                    // todo: check if it's possible that it's from the model. If it is possible, we need to add support for model schemas
                    if (originalTableEntitySource.Schema != null)
                        return;

                    originalTableName = originalTableEntitySource.Entity;
                }
                else
                {
                    originalTableName = sourceRef.Entity;
                }

                var tableMapping = renames.Table(originalTableName);
                if (renames.TryGetMappedObjectInfo(originalTableName, expression.Property, out var objectMapping))
                {
                    var originalName = expression.Property;
                    var newName = objectMapping.NewObjectName;
                    expression.Property = newName;
                    // if it was a direct reference, replace the source name as well (the original table might not have been replaced by the new table,
                    // it's possible that just this one column was moved to the new table so the sourceRef would not have been replaced)
                    if (sourceRef.Entity != null)
                        sourceRef.Entity = objectMapping.NewTableName;

                    aliasedPropertyExpressions
                        .GetValueOrInitialize(objectMapping.NewTableName, _ => new List<QueryExpression>())
                        .Add(expression);
                }
                else
                {
                    // use tableMapping.NewTableName because the owning table name might have been changed
                    aliasedPropertyExpressions
                        .GetValueOrInitialize(tableMapping.NewTableName, _ => new List<QueryExpression>())
                        .Add(expression);
                }
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {
                base.Visit(expression);

                var sourceRef = expression.Expression.SourceRef;

                if (sourceRef == null)
                    return;

                if (sourceRef.Source != null)
                {
                    var originalTableEntitySource = SourcesByAliasMap[sourceRef.Source];
                    // skipping over the ones that have schema set, those are extensions (report-defined measures)
                    // todo: check if it's possible that it's from the model. If it is possible, we need to add support for model schemas
                    if (originalTableEntitySource.Schema != null)
                        return;

                    var tableMapping = renames.Table(originalTableEntitySource.Entity);
                    if (tableMapping.TryGetMapping(expression.Hierarchy, out var objectMapping))
                    {
                        var originalName = expression.Hierarchy;
                        var newName = objectMapping.NewObjectName;
                        expression.Hierarchy = newName;

                        aliasedPropertyExpressions
                            .GetValueOrInitialize(objectMapping.NewTableName, _ => new List<QueryExpression>())
                            .Add(expression);
                    }
                    else
                    {
                        // use tableMapping.NewTableName because the owning table name might have been changed
                        aliasedPropertyExpressions
                            .GetValueOrInitialize(tableMapping.NewTableName, _ => new List<QueryExpression>())
                            .Add(expression);
                    }
                }
            }

            private string FindNextAvailableAlias(string v, HashSet<string> takenNames)
            {
                if (!takenNames.Contains(v))
                    return v;
                else
                {
                    for (int i = 1; ; i++)
                    {
                        string candidate = $"{v}{i}";
                        if (!takenNames.Contains(candidate))
                            return candidate;
                    }
                }
            }
        }
    }
}
