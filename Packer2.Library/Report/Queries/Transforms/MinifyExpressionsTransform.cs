using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.MinifiedQueryParser.QueryTransforms
{
    public class ColumnsAndMeasuresGlossary
    {
        public Dictionary<string, PerTableColumnsMeasuresGlossary> TableGlossaries { get; set; } = new Dictionary<string, PerTableColumnsMeasuresGlossary>();

        public void AddColumn(string tableName, string columnName)
        {
            PerTableColumnsMeasuresGlossary tableGlossary;
            if (TableGlossaries.TryGetValue(tableName, out tableGlossary) == false)
                TableGlossaries[tableName] = tableGlossary = new PerTableColumnsMeasuresGlossary();

            tableGlossary.Columns.Add(columnName);
        }

        public bool IsMeasure(string tableName, string propertyName)
        {
            return TableGlossaries.TryGetValue(tableName, out var tg) && tg.Measures.Contains(propertyName);
        }

        public bool IsColumn(string tableName, string propertyName)
        {
            return TableGlossaries.TryGetValue(tableName, out var tg) && tg.Columns.Contains(propertyName);
        }

        public void AddMeasure(string tableName, string measureName)
        {
            PerTableColumnsMeasuresGlossary tableGlossary;
            if (TableGlossaries.TryGetValue(tableName, out tableGlossary) == false)
                TableGlossaries[tableName] = tableGlossary = new PerTableColumnsMeasuresGlossary();

            tableGlossary.Measures.Add(measureName);
        }

        public class PerTableColumnsMeasuresGlossary
        {
            public HashSet<string> Columns { get; set; } = new HashSet<string>();
            public HashSet<string> Measures { get; set; } = new HashSet<string>();
        }
    }


    class MinifyExpressionsLayoutJsonTransform : ReportInfoNavTransformBase
    {
        public ColumnsAndMeasuresGlossary Glossary { get; } = new ColumnsAndMeasuresGlossary();

        // todo: use visitor to populate dictionary of measures and columns that we will then save to a file (outside of this class)

        protected override QueryExpressionVisitor CreateProcessingVisitor(string outerPath, string innerPath, Dictionary<string, string> sourceByAliasMap = null)
            => new DetectMeasuresAndColumnsVisitor(outerPath, innerPath, sourceByAliasMap, Glossary);

        protected override bool TryReadExpression(JToken expToken, out QueryExpressionContainer? expressionContainer)
        {
            var res = base.TryReadExpression(expToken, out expressionContainer);
            if (res == true)
            {
                res = !expressionContainer.ToString().Contains("#error#");
                // do not use minification on expressions that report errors on minimization
                if (expressionContainer.ToString().Contains("#error#"))
                {
                    res = false;
                }
            }
            return res;
        }

        protected override bool TryReadFilter(JToken expToken, out FilterDefinition? filter)
        {
            var res = base.TryReadFilter(expToken, out filter);
            if (res == true)
            {
                // do not use minification on filters that report errors on minimization
                if (filter.ToString().Contains("#error#"))
                {
                    res = false;
                }
            }
            return res;
        }

        protected override bool TryReadQuery(JToken expToken, out QueryDefinition? queryDefinition)
        {
            var res = base.TryReadQuery(expToken, out queryDefinition);
            if (res == true)
            {
                // do not use minification on queries that report errors on minimization
                if (queryDefinition.ToString().Contains("#error#"))
                {
                    res = false;
                }
            }
            return res;
        }

        protected override void WriteExpression(JToken expToken, QueryExpressionContainer expObj)
        {
            expToken.Replace(expObj.ToString());
        }

        protected override void WriteFilter(JToken expToken, FilterDefinition filterObj)
        {
            expToken.Replace(filterObj.ToString());
        }

        protected override void WriteQuery(JToken expToken, QueryDefinition queryObj)
        {
            // attach select list next to this property
            var selectList = queryObj.Select.Select(ec => ec.Name).ToArray();
            var parent = (JProperty)expToken.Parent!;
            parent!.AddAfterSelf(new JProperty($"#{parent.Name}SelectList", selectList));

            expToken.Replace(queryObj.ToString());
        }

        class DetectMeasuresAndColumnsVisitor : BaseQueryExpressionVisitor
        {
            private readonly Dictionary<string, string> sourcesByAliasMap;
            private readonly ColumnsAndMeasuresGlossary glossary;

            public DetectMeasuresAndColumnsVisitor(string outerPath, string innerPath, Dictionary<string, string> sourcesByAliasMap, ColumnsAndMeasuresGlossary glossary) 
                : base(outerPath, innerPath, sourcesByAliasMap)
            {
                this.sourcesByAliasMap = sourcesByAliasMap;
                this.glossary = glossary;
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                if (expression.Expression.SourceRef != null)
                {
                    var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source];
                    glossary.AddMeasure(sourceName, expression.Property);
                }
                base.Visit(expression);
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                if (expression.Expression.SourceRef != null)
                {
                    var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source];
                    glossary.AddColumn(sourceName, expression.Property);
                }
                base.Visit(expression);
            }
        }
    }
}
