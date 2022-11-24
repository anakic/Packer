using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.MinifiedQueryParser.QueryTransforms;

namespace Packer2.Library.Report.QueryTransforms.Antlr
{
    public partial class QueryParser
    {
        class QueryConstructorVisitor : pbiqParserBaseVisitor<QueryDefinition>
        {
            private readonly ColumnsAndMeasuresGlossary glossary;
            private readonly ParserResultValidator validator;

            private string ReadStringLiteral(ITerminalNode node)
            {
                if (node == null)
                    return null;

                var text = node.GetText();
                return text.Substring(1, text.Length - 2);
            }

            public QueryTransform ParseTransform(pbiqParser.TransformContext context, Dictionary<string, string> sourceNames, HashSet<string> transformTables)
            {
                QueryTransformInput input = new QueryTransformInput();
                QueryTransformOutput output = new QueryTransformOutput();

                if (context.parameters() != null)
                    input.Parameters = context.parameters().parameter().Select(pc => ParseExprContainer(pc, sourceNames, transformTables, UnescapeIdentifier(pc.alias().GetText()))).ToList();

                if (context.inputTable() != null)
                {
                    input.Table = new QueryTransformTable()
                    {
                        Name = context.inputTable().alias().GetText(),
                        Columns = context.inputTable().tableColumn().Select(tc => new QueryTransformTableColumn() { Role = ReadStringLiteral(tc.STRING_LITERAL()), Expression = ParseExprContainer(tc.expression(), sourceNames, transformTables, UnescapeIdentifier(tc.alias().GetText())) }).ToList()
                    };
                    transformTables.Add(input.Table.Name);
                }

                if (context.outputTable() != null)
                {
                    output.Table = new QueryTransformTable()
                    {
                        Name = context.outputTable().alias().GetText(),
                        Columns = context.outputTable().tableColumn().Select(tc => new QueryTransformTableColumn() { Role = ReadStringLiteral(tc.STRING_LITERAL()), Expression = ParseExprContainer(tc.expression(), sourceNames, transformTables, UnescapeIdentifier(tc.alias().GetText())) }).ToList()
                    };
                    transformTables.Add(output.Table.Name);
                }

                return new QueryTransform()
                {
                    Name = UnescapeIdentifier(context.identifier().GetText()),
                    Input = input,
                    Output = output,
                    Algorithm = ReadStringLiteral(context.algorithm().STRING_LITERAL())
                };
            }

            public QueryConstructorVisitor(ColumnsAndMeasuresGlossary glossary, ParserResultValidator validator)
            {
                this.glossary = glossary;
                this.validator = validator;
            }

            public override QueryDefinition VisitRoot([NotNull] pbiqParser.RootContext context)
            {
                return VisitQuery(context.query());
            }

            public override QueryDefinition VisitQuery([NotNull] pbiqParser.QueryContext context)
            {
                var def = new QueryDefinition() { Version = 2 };
                if (context.from() != null)
                    def.From = context.from().fromElement().Select(fe =>
                    {
                        var source = new EntitySource();
                        source.Name = fe.alias().GetText();

                        if (fe.entity_name() != null)
                            source.Entity = UnescapeIdentifier(fe.entity_name().GetText());

                        else
                        {
                            var subQuery = ParseExprContainer(fe.subQueryExpr());
                            source.Expression = subQuery;
                            source.Type = EntitySourceType.Expression;
                        }

                        return source;
                    }).ToList();

                var sourceNames = def?.From.ToDictionary(f => f.Name, f => f.Entity) ?? new Dictionary<string, string>();

                HashSet<string> transformNames = new HashSet<string>();
                if (context.transform()?.Count() > 0)
                    def.Transform = context.transform().Select(t => ParseTransform(t, sourceNames, transformNames)).ToList();

                if (context.where() != null)
                    def.Where = context.where().queryFilterElement().Select(qf => new QueryFilter() { Condition = ParseExprContainer(qf, sourceNames, transformNames) }).ToList();


                if (context.select() != null)
                    def.Select = context.select().expression().Select((exp, i) => { var expCont = ParseExprContainer(exp, sourceNames, transformNames); return expCont; }).ToList();

                if (context.orderby() != null)
                    def.OrderBy = context.orderby().orderbySection().Select(obs => new QuerySortClause() { Expression = ParseExprContainer(obs.expression(), sourceNames, transformNames), Direction = ParseDirection(obs.direction().GetText()) }).ToList();

                if (context.groupby() != null)
                    def.GroupBy = context.groupby().expression().Select(exp => ParseExprContainer(exp, sourceNames, transformNames)).ToList();

                if (context.skip() != null)
                    def.Skip = int.Parse(context.skip().INTEGER().GetText());

                if (context.top() != null)
                    def.Top = int.Parse(context.top().INTEGER().GetText());

                var asJson = Newtonsoft.Json.JsonConvert.SerializeObject(def, Newtonsoft.Json.Formatting.Indented);

                return def;
            }

            private QueryExpressionContainer ParseExprContainer(IParseTree context, Dictionary<string, string>? sourceNames = null, HashSet<string> transformTableNames = null, string alias = null)
            {
                var expression = new QueryExpressionVisitor(glossary, validator, false, sourceNames, transformTableNames).VisitValidated(context);
                var container = new QueryExpressionContainer(expression);
                if (alias != null)
                    container.Name = alias;
                return container;
            }

            private QuerySortDirection ParseDirection(string v)
            {
                switch (v?.ToLower())
                {
                    case "ascending":
                        return QuerySortDirection.Ascending;
                    case "descending":
                        return QuerySortDirection.Descending;
                    default:
                        return QuerySortDirection.Unspecified;
                }
            }
        }
    }
}
