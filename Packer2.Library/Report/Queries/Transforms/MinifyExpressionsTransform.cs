using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json.Linq;
using Packer2.Library.Report.Queries;
using Packer2.Library.Report.Transforms;
using System.Linq.Expressions;

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
        EnsureQutedNameVisitor aliasFixVisitor = new EnsureQutedNameVisitor();

        /// <summary>
        /// This fixes an issue with the built-in query/filter/expression .ToString method
        /// where the alias is chosed as the first character of the table name, even if it's
        /// a non-letter char, but that alias is never quoted.
        /// </summary>
        /// <example>
        /// Because of the bug we might have something like this:
        /// from * in [*Age Band_Dim] where not(*.[Age Band] in (null, '0-10', '11-17', '75+', '65-74'))
        /// 
        /// This fixes the problem by converting to this prior to minification:
        /// from ZZasteriskZZ in [*Age Band_Dim] where not(ZZasteriskZZ.[Age Band] in (null, '0-10', '11-17', '75+', '65-74'))
        /// </example>
        class EnsureQutedNameVisitor : ExtendedExpressionVisitor
        {
            public override void Visit(QueryDefinition queryDefinition)
            {
                queryDefinition.From.ForEach(fr =>
                    fr.Name = FixName(fr.Name));

                base.Visit(queryDefinition);
            }

            public override void Visit(FilterDefinition filterDefinition)
            {
                filterDefinition.From.ForEach(fr =>
                    fr.Name = FixName(fr.Name));

                base.Visit(filterDefinition);
            }

            protected override void Visit(QuerySourceRefExpression expression)
            {
                expression.Source = FixName(expression.Source);
                base.Visit(expression);
            }

            static string? FixName(string? name)
            {
                if (name == null)
                    return null;

                var c = name[0];
                if (map.TryGetValue((int)c, out var renamed))
                {
                    name = renamed + name.Substring(1);
                }

                return name;
            }

            static Dictionary<int, string> map = new Dictionary<int, string>()
            {
                { 0, "ZZnull_characterZZ" },
                { 1, "ZZstart_of_headingZZ" },
                { 2, "ZZstart_of_textZZ" },
                { 3, "ZZend_of_text_characterZZ" },
                { 4, "ZZend_of_transmission_characterZZ" },
                { 5, "ZZenquiry_characterZZ" },
                { 6, "ZZacknowledge_characterZZ" },
                { 7, "ZZbell_characterZZ" },
                { 8, "ZZbackspaceZZ" },
                { 9, "ZZhorizontal_tabZZ" },
                { 10, "ZZline_feedZZ" },
                { 11, "ZZvertical_tabZZ" },
                { 12, "ZZform_feedZZ" },
                { 13, "ZZcarriage_returnZZ" },
                { 14, "ZZshift_outZZ" },
                { 15, "ZZshift_inZZ" },
                { 16, "ZZdata_link_escapeZZ" },
                { 17, "ZZdevice_controlZZZZ" },
                { 18, "ZZdevice_controlZZZZ" },
                { 19, "ZZdevice_controlZZZZ" },
                { 20, "ZZdevice_controlZZZZ" },
                { 21, "ZZnegative_acknowledge_characterZZ" },
                { 22, "ZZsynchronous_idleZZ" },
                { 23, "ZZend_of_transmission_blockZZ" },
                { 24, "ZZcancel_characterZZ" },
                { 25, "ZZend_of_mediumZZ" },
                { 26, "ZZsubstitute_characterZZ" },
                { 27, "ZZescape_characterZZ" },
                { 28, "ZZfile_separatorZZ" },
                { 29, "ZZgroup_separatorZZ" },
                { 30, "ZZrecord_separatorZZ" },
                { 31, "ZZunit_separatorZZ" },
                { 32, "ZZspaceZZ" },
                { 33, "ZZexclamation_markZZ" },
                { 34, "ZZquotation_markZZ" },
                { 35, "ZZnumber_signZZ" },
                { 36, "ZZdollar_signZZ" },
                { 37, "ZZpercent_signZZ" },
                { 38, "ZZampersandZZ" },
                { 39, "ZZapostropheZZ" },
                { 40, "ZZleft_parenthesisZZ" },
                { 41, "ZZright_parenthesisZZ" },
                { 42, "ZZasteriskZZ" },
                { 43, "ZZplus_signZZ" },
                { 44, "ZZcommaZZ" },
                { 45, "ZZhyphen_minusZZ" },
                { 46, "ZZfull_stopZZ" },
                { 47, "ZZslashZZ" },
                { 58, "ZZcolonZZ" },
                { 59, "ZZsemicolonZZ" },
                { 60, "ZZless_than_signZZ" },
                { 61, "ZZequal_signZZ" },
                { 62, "ZZgreater_than_signZZ" },
                { 63, "ZZquestion_markZZ" },
                { 64, "ZZat_signZZ" },
                { 91, "ZZleft_square_bracketZZ" },
                { 92, "ZZbackslashZZ" },
                { 93, "ZZright_square_bracketZZ" },
                { 94, "ZZcircumflex_accentZZ" },
                // { 95, "ZZlow_lineZZ" },
                { 96, "ZZgrave_accentZZ" },
                { 123, "ZZleft_curly_bracketZZ" },
                { 124, "ZZvertical_barZZ" },
                { 125, "ZZright_curly_bracketZZ" },
                { 126, "ZZtildeZZ" },
                { 127, "ZZdeleteZZ" }
            };
        }

        public MinifyExpressionsLayoutJsonTransform(ILogger logger)
            : base(logger)
        {
        }

        public ColumnsAndMeasuresGlossary Glossary { get; } = new ColumnsAndMeasuresGlossary();

        protected override void Process(FilterDefinition filter, string path)
        {
            // just register the columns and measures into the glossary here,
            // we'll minify when writing
            var detections = filter.DetectReferences();
            AddToGlossary(detections);
        }

        protected override void Process(QueryDefinition query, string path)
        {
            // just register the columns and measures into the glossary here
            // we'll minify when writing
            var detections = query.DetectReferences();
            AddToGlossary(detections);
        }

        protected override void Process(QueryExpressionContainer expression, string path)
        {
            // just register the columns and measures into the glossary here
            // we'll minify when writing
            var detections = expression.DetectReferences();
            AddToGlossary(detections);
        }

        private void AddToGlossary(Detections detections)
        {
            foreach (var m in detections.MeasureReferences.GroupBy(x => new { x.TableName, x.Measure }))
                Glossary.AddMeasure(m.Key.TableName, m.Key.Measure);
            foreach (var m in detections.ColumnReferences.GroupBy(x => new { x.TableName, x.Column }))
                Glossary.AddColumn(m.Key.TableName, m.Key.Column);
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
            aliasFixVisitor.Visit(filterObj);
            expToken.Replace(filterObj.ToString());
        }

        protected override void WriteQuery(JToken expToken, QueryDefinition queryObj)
        {
            aliasFixVisitor.Visit(queryObj);

            // attach select list next to this property
            var selectList = queryObj.Select.Select(ec => ec.Name).ToArray();
            var parent = (JProperty)expToken.Parent!;
            parent!.AddAfterSelf(new JProperty($"#{parent.Name}SelectList", selectList));

            expToken.Replace(queryObj.ToString());
        }
    }
}
