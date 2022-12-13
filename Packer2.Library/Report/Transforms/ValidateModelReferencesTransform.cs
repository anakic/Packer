using Microsoft.AnalysisServices.Tabular;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Queries;
using Packer2.Library.Tools;

namespace Packer2.Library.Report.Transforms
{
    public class ValidateModelReferencesTransform : ReportInfoNavTransformBase
    {
        int errorCount;
        private readonly Database database;
        private readonly ILogger<ValidateModelReferencesTransform> logger;

        // todo: replace with logger?
        public ValidateModelReferencesTransform(Database database, ILogger<ValidateModelReferencesTransform>? logger = null)
        {
            this.database = database;
            this.logger = logger ?? new DummyLogger<ValidateModelReferencesTransform>();
        }

        protected override ExtendedExpressionVisitor CreateProcessingVisitor(string path)
            => new DetectRefErrorsVisitor(path, database, logger, () => errorCount++);

        protected override void OnProcessingComplete(JObject jObj)
        {
            if (errorCount == 0)
                logger.LogInformation("No validation errors detected");
            else
                throw new Exception($"### A total of {errorCount} validation errors have been detected. ###");
        }


        class DetectRefErrorsVisitor : BaseTransformVisitor
        {
            public HashSet<string>? SourcesToIgnore { get; set; } = new HashSet<string>();

            private readonly Database db;
            private readonly ILogger traceRefErrorReporter;
            private readonly Action incrementErrorCount;

            public DetectRefErrorsVisitor(string path, Database db, ILogger traceRefErrorReporter, Action incrementErrorCount)
                : base(path)
            {
                this.db = db;
                this.traceRefErrorReporter = traceRefErrorReporter;
                this.incrementErrorCount = incrementErrorCount;
            }

            protected override void Visit(QueryHierarchyExpression expression)
            {
                TryDo(() =>
                {
                    var visitor = new ResolveTabularExpressionTargetVisitor<Table, Model>(db.Model.Tables, SourcesByAliasMap);
                    return (Hierarchy)expression.Accept(visitor);
                });
            }

            protected override void Visit(QueryHierarchyLevelExpression expression)
            {
                TryDo(() =>
                {
                    var visitor = new ResolveTabularExpressionTargetVisitor<Table, Model>(db.Model.Tables, SourcesByAliasMap);
                    return (Level)expression.Accept(visitor);
                });
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                TryDo(() =>
                {
                    var visitor = new ResolveTabularExpressionTargetVisitor<Table, Model>(db.Model.Tables, SourcesByAliasMap);
                    return (Column)expression.Accept(visitor);
                });
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                TryDo(() =>
                {
                    var visitor = new ResolveTabularExpressionTargetVisitor<Table, Model>(db.Model.Tables, SourcesByAliasMap);
                    return (Measure)expression.Accept(visitor);
                });
            }

            private void TryDo(Func<MetadataObject> function)
            {
                try
                {
                    var result = function();
                    if (result == null)
                        throw new ResolutionException("Resolved object was null, but was expecting an instance... This should never happen. Investigate Packer2 code at this location.");
                }
                catch (Exception ex)
                {
                    traceRefErrorReporter.LogError("{errorMessage}. Path: '{path}'", ex.Message, Path);
                    incrementErrorCount();
                }
            }
        }

        public class ResolutionException : Exception
        {
            public ResolutionException(string errorMessage)
                : base(errorMessage)
            {

            }
        }

        class ResolveTabularExpressionTargetVisitor<T, K> : QueryExpressionVisitor<NamedMetadataObject> where T : NamedMetadataObject where K : MetadataObject
        {
            private readonly NamedMetadataObjectCollection<T, K> parent;
            private readonly Dictionary<string, string> sourcesMap;

            public ResolveTabularExpressionTargetVisitor(NamedMetadataObjectCollection<T, K> parent, Dictionary<string, string> sourcesMap)
            {
                this.parent = parent;
                this.sourcesMap = sourcesMap;
            }

            protected override NamedMetadataObject Visit(QuerySourceRefExpression expression)
            {
                string? entity = expression.Entity;
                if (entity == null)
                {
                    if (expression.Source != null)
                    {
                        if (sourcesMap.TryGetValue(expression.Source, out entity) == false)
                            throw new ResolutionException($"Invalid source alias '{expression.Source}'!");
                    }
                    else
                    {
                        throw new ResolutionException("Invalid sourceRef expression!");
                    }
                }

                if (parent.Contains(entity))
                    return parent[entity];
                else
                    throw new ResolutionException($"Table '{entity}' not found");
            }

            protected override NamedMetadataObject Visit(QueryPropertyExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryColumnExpression expression)
            {
                var table = (Table)Visit(expression.Expression.SourceRef);
                if (table.Columns.Contains(expression.Property))
                    return table.Columns[expression.Property];
                else
                    throw new ResolutionException($"Column '{expression.Property}' not found in table '{table.Name}'");
            }

            protected override NamedMetadataObject Visit(QueryMeasureExpression expression)
            {
                var table = (Table)Visit(expression.Expression.SourceRef);
                if (table.Measures.Contains(expression.Property))
                    return table.Measures[expression.Property];
                else
                    throw new ResolutionException($"Measure '{expression.Property}' not found in table '{table.Name}'");
            }

            protected override NamedMetadataObject Visit(QueryHierarchyExpression expression)
            {
                var hierarchyOwner = expression.Expression.Expression.Accept(this);
                if (hierarchyOwner is Variation v)
                {
                    if (v.DefaultHierarchy.Name != expression.Hierarchy)
                        throw new ResolutionException($"Invalid hierarchy in variation '{v.Name}' in column '{v.Column.Name}' of table '{v.Column.Table.Name}'. Expecting name '{v.DefaultColumn.Name}' but found '{expression.Hierarchy}'.");
                    else
                        return v.DefaultHierarchy;
                }
                else if (hierarchyOwner is Table t)
                {
                    var table = (Table)hierarchyOwner;
                    if (table.Hierarchies.Contains(expression.Hierarchy))
                        return table.Hierarchies[expression.Hierarchy];
                    else
                        throw new ResolutionException($"Hierarchy '{expression.Hierarchy}' not found in table '{table.Name}'");
                }
                else
                    throw new NotImplementedException("Unexpected hierarchy owner type. Inspect packer2 code and update as necessary.");
            }


            protected override NamedMetadataObject Visit(QueryHierarchyLevelExpression expression)
            {
                var hierarchy = (Hierarchy)expression.Expression.Expression.Accept(this);
                return hierarchy.Levels[expression.Level];
            }

            protected override NamedMetadataObject Visit(QueryPropertyVariationSourceExpression expression)
            {
                var table = (Table)expression.Expression.Expression.Accept(this);
                return table.Columns[expression.Property].Variations[expression.Name];
            }

            protected override NamedMetadataObject Visit(QueryNativeVisualCalculationExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryAggregationExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDatePartExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryPercentileExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryFloorExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDiscretizeExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryMemberExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryNativeFormatExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryNativeMeasureExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryExistsExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryNotExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryAndExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryOrExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryComparisonExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryContainsExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryStartsWithExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryArithmeticExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryEndsWithExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryBetweenExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryInExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryScopedEvalExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryFilteredEvalExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QuerySparklineDataExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryLiteralExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDefaultValueExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryAnyValueExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryBooleanConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDateConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDateTimeConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDateTimeSecondConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDecadeConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDecimalConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryIntegerConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryNullConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryStringConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryNumberConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryYearAndMonthConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryYearAndWeekConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryYearConstantExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryNowExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDateAddExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryDateSpanExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryTransformOutputRoleRefExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryTransformTableRefExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QuerySubqueryExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryLetRefExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryRoleRefExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QuerySummaryValueRefExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryParameterRefExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryTypeOfExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryPrimitiveTypeExpression expression)
            {
                throw new NotImplementedException();
            }

            protected override NamedMetadataObject Visit(QueryTableTypeExpression expression)
            {
                throw new NotImplementedException();
            }
        }
    }
}
