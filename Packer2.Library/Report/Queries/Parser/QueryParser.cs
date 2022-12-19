using Antlr4.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.MinifiedQueryParser.QueryTransforms;
using Packer2.Library.Tools;
using System.Text.RegularExpressions;

namespace Packer2.Library.Report.QueryTransforms.Antlr
{
    public partial class QueryParser
    {
        private readonly IDbInfoGetter dbInfoGetter;
        private readonly ParserResultValidator validator;

        public QueryParser(IDbInfoGetter dbInfoGetter, ILogger? logger = null)
        {
            this.dbInfoGetter = dbInfoGetter;
            validator = new ParserResultValidator(logger ?? new DummyLogger<QueryParser>());
        }

        public QueryDefinition ParseQuery(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.queryRoot();

            var queryDefinition = new QueryConstructorVisitor(dbInfoGetter, validator).Visit(tree);
            validator.ValidateQuery(queryDefinition);

            if (NormalizeNewlines(input) != NormalizeNewlines(queryDefinition.ToString()))
                throw new FormatException($"Parsing query did not throw an exception but the constructed filter does not match the input string. Input string was '{input}', while the minimized constructed filter was '{queryDefinition}'!");

            return queryDefinition;
        }

        Regex wsRx = new Regex(@"(\r|\n)+", RegexOptions.Compiled);
        private string NormalizeNewlines(string input)
            => wsRx.Replace(input, " ");

        public FilterDefinition ParseFilter(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.queryRoot();
            var queryDefinition = new QueryConstructorVisitor(dbInfoGetter, validator).Visit(tree);

            if (queryDefinition.Select?.Count > 0 && queryDefinition.Parameters?.Count > 0 || queryDefinition.Transform?.Count > 0 || queryDefinition.GroupBy?.Count > 0 || queryDefinition.Let?.Count > 0 || queryDefinition.OrderBy?.Count > 0 || queryDefinition.Skip != null || queryDefinition.Top != null)
                throw new FormatException("Was expecting filter but got query");

            var filter = new FilterDefinition()
            {
                Version = queryDefinition.Version,
                From = queryDefinition.From,
                Where = queryDefinition.Where
            };

            validator.ValidateFilter(filter);

            if (input != filter.ToString())
                throw new FormatException($"Parsing filter did not throw an exception but the constructed filter does not match the input string. Input string was '{input}', while the minimized constructed filter was '{filter}'!");

            return filter;
        }

        public QueryExpression ParseExpression(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.expressionContainer();

            var expression = new QueryExpressionVisitor(dbInfoGetter, validator, true, new Dictionary<string, string>(), new HashSet<string>()).VisitValidated(tree);

            if (input != expression.ToString())
                throw new FormatException($"Parsing expression did not throw an exception but the constructed expression does not match the input string. Input string was '{input}', while the minimized constructed expression was '{expression}'!");

            return expression;
        }

        private pbiqParser CreateParser(string input)
        {
            var lexer = new pbiqLexer(CharStreams.fromString(input));
            return new pbiqParser(new CommonTokenStream(lexer));
        }

        static string? UnescapeIdentifier(string? identifier)
        {
            if (identifier == null)
                return null;

            identifier = identifier.Trim();
            if (identifier.StartsWith("["))
            {
                if (identifier.EndsWith("]") == false)
                    throw new FormatException("Invalid identifier");

                identifier = identifier.Substring(1, identifier.Length - 2);
                identifier = identifier.Replace("]]", "]");
            }
            return identifier;
        }
    }
}
