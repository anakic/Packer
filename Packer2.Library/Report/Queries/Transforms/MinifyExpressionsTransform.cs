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
        /// from __asterisk__ in [*Age Band_Dim] where not(__asterisk__.[Age Band] in (null, '0-10', '11-17', '75+', '65-74'))
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
                { 0, "__null_character__" },
                { 1, "__start_of_heading__" },
                { 2, "__start_of_text__" },
                { 3, "__end_of_text_character__" },
                { 4, "__end_of_transmission_character__" },
                { 5, "__enquiry_character__" },
                { 6, "__acknowledge_character__" },
                { 7, "__bell_character__" },
                { 8, "__backspace__" },
                { 9, "__horizontal_tab__" },
                { 10, "__line_feed__" },
                { 11, "__vertical_tab__" },
                { 12, "__form_feed__" },
                { 13, "__carriage_return__" },
                { 14, "__shift_out__" },
                { 15, "__shift_in__" },
                { 16, "__data_link_escape__" },
                { 17, "__device_control____" },
                { 18, "__device_control____" },
                { 19, "__device_control____" },
                { 20, "__device_control____" },
                { 21, "__negative_acknowledge_character__" },
                { 22, "__synchronous_idle__" },
                { 23, "__end_of_transmission_block__" },
                { 24, "__cancel_character__" },
                { 25, "__end_of_medium__" },
                { 26, "__substitute_character__" },
                { 27, "__escape_character__" },
                { 28, "__file_separator__" },
                { 29, "__group_separator__" },
                { 30, "__record_separator__" },
                { 31, "__unit_separator__" },
                { 32, "__space__" },
                { 33, "__exclamation_mark__" },
                { 34, "__quotation_mark__" },
                { 35, "__number_sign__" },
                { 36, "__dollar_sign__" },
                { 37, "__percent_sign__" },
                { 38, "__ampersand__" },
                { 39, "__apostrophe__" },
                { 40, "__left_parenthesis__" },
                { 41, "__right_parenthesis__" },
                { 42, "__asterisk__" },
                { 43, "__plus_sign__" },
                { 44, "__comma__" },
                { 45, "__hyphen_minus__" },
                { 46, "__full_stop__" },
                { 47, "__slash__" },
                { 58, "__colon__" },
                { 59, "__semicolon__" },
                { 60, "__less_than_sign__" },
                { 61, "__equal_sign__" },
                { 62, "__greater_than_sign__" },
                { 63, "__question_mark__" },
                { 64, "__at_sign__" },
                { 91, "__left_square_bracket__" },
                { 92, "__backslash__" },
                { 93, "__right_square_bracket__" },
                { 94, "__circumflex_accent__" },
                // { 95, "__low_line__" },
                { 96, "__grave_accent__" },
                { 123, "__left_curly_bracket__" },
                { 124, "__vertical_bar__" },
                { 125, "__right_curly_bracket__" },
                { 126, "__tilde__" },
                { 127, "__delete__" }
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
            var xxx = filterObj.ToString();
            if (xxx.Contains(@"[*Master Index Metrics]"))
            {
                var yyy = filterObj.ToString();
            }

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
