using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Queries;
using Packer2.Library.Report.Transforms;

namespace Packer2.Library.MinifiedQueryParser.QueryTransforms
{
    public interface IDbInfoGetter
    {
        bool IsMeasure(string tableName, string propertyName);
        bool IsColumn(string tableName, string propertyName);
    }

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

    // todo: this requires unstuffed json, do not inherit from ReportInfoNavTransformBase,
    // refactor to ensure we cannot call this on the stuffed json. E.g. make a composite ReportInfoNavTransformBase
    // that will perform a series of transformations on the unstuffed version of the layout json file.
    class MinifyExpressionsLayoutJsonTransform : ReportInfoNavTransformBase
    {
        public MinifyExpressionsLayoutJsonTransform(ILogger logger)
            : base(logger)
        {
        }

        public ColumnsAndMeasuresGlossary Glossary { get; } = new ColumnsAndMeasuresGlossary();

        protected override void Process(FilterDefinition filter, string path)
        {
            // just register the columns and measures into the glossary here,
            // we'll minify when writing
            var visitor = new AddMeasuresAndColumnsToGlossaryVisitor(Glossary);
            visitor.Visit(filter);
        }

        protected override void Process(QueryDefinition query, string path)
        {
            // just register the columns and measures into the glossary here
            // we'll minify when writing
            var visitor = new AddMeasuresAndColumnsToGlossaryVisitor(Glossary);
            visitor.Visit(query);
        }

        protected override void Process(QueryExpressionContainer expression, string path)
        {
            // just register the columns and measures into the glossary here
            // we'll minify when writing
            var visitor = new AddMeasuresAndColumnsToGlossaryVisitor(Glossary);
            visitor.VisitExpression(expression);
        }

        protected override bool TryReadExpression(JToken expToken, out QueryExpressionContainer? expressionContainer)
        {
            var res = base.TryReadExpression(expToken, out expressionContainer);
            if (res == true)
            {
                string str = expressionContainer.ToString();

                res = !str.Contains("#error#");
                // do not use minification on expressions that report errors on minimization
                if (expressionContainer.ToString().Contains("#error#"))
                {
                    res = false;
                }
                else
                {
                    logger.LogDebug("Read minified expression '{expression}'", str);
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

        class AddMeasuresAndColumnsToGlossaryVisitor : BaseTransformVisitor
        {
            private readonly ColumnsAndMeasuresGlossary glossary;

            public AddMeasuresAndColumnsToGlossaryVisitor(ColumnsAndMeasuresGlossary glossary) 
            {
                this.glossary = glossary;
            }

            protected override void Visit(QueryMeasureExpression expression)
            {
                if (expression.Expression.SourceRef != null)
                {
                    var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source].Entity;
                    glossary.AddMeasure(sourceName, expression.Property);
                }
                base.Visit(expression);
            }

            protected override void Visit(QueryColumnExpression expression)
            {
                if (expression.Expression.SourceRef != null)
                {
                    var sourceName = expression.Expression.SourceRef.Entity ?? SourcesByAliasMap[expression.Expression.SourceRef.Source].Entity;
                    glossary.AddColumn(sourceName, expression.Property);
                }
                base.Visit(expression);
            }
        }
    }
}
