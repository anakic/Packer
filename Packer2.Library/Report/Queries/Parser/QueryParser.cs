using Antlr4.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Newtonsoft.Json;

namespace Packer2.Library.Report.QueryTransforms.Antlr
{
    public partial class QueryParser
    {
        public const string MEASURES_PREFIX = "M__";
        public const string COLUMNS_PREFIX = "C__";

        private readonly ILogger logger;
        private readonly ParserResultValidator validator;
        
        AddMissingMetadataVisitor addMissingMetadataVisitor = new AddMissingMetadataVisitor();
        ClearAddedMetadataVisitor clearAddedMetadataVisitor = new ClearAddedMetadataVisitor();

        public QueryParser(ILogger logger)
        {
            this.logger = logger;
            validator = new ParserResultValidator(logger);
        }

        public QueryDefinition ParseQuery(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.root();

            var queryDefinition = new QueryConstructorVisitor(validator).Visit(tree);
            validator.ValidateQuery(queryDefinition);

            if (input != queryDefinition.ToString())
                throw new FormatException($"Parsing query did not throw an exception but the constructed filter does not match the input string. Input string was '{input}', while the minimized constructed filter was '{queryDefinition}'!");

            addMissingMetadataVisitor.Visit(queryDefinition);

            return queryDefinition;
        }

        public FilterDefinition ParseFilter(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.root();
            var queryDefinition = new QueryConstructorVisitor(validator).Visit(tree);

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

            addMissingMetadataVisitor.Visit(queryDefinition);

            return filter;
        }

        public QueryExpression ParseExpression(string input)
        {
            var parser = CreateParser(input);
            var tree = parser.expressionContainer();

            var expression = new QueryExpressionVisitor(validator, true).VisitValidated(tree);

            if (input != expression.ToString())
                throw new FormatException($"Parsing expression did not throw an exception but the constructed expression does not match the input string. Input string was '{input}', while the minimized constructed expression was '{expression}'!");

            addMissingMetadataVisitor.Visit(expression);

            return expression;
        }

        private pbiqParser CreateParser(string input)
        {
            var lexer = new pbiqLexer(CharStreams.fromString(input));
            return new pbiqParser(new CommonTokenStream(lexer));
        }

        static string UnescapeIdentifier(string identifier)
        {
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

        public string Format(QueryDefinition query)
        {
            var clone = Clone(query);
            clearAddedMetadataVisitor.Visit(clone);
            return clone.ToString();
        }

        public string Format(FilterDefinition filter)
        {
            var clone = Clone(filter);
            clearAddedMetadataVisitor.Visit(clone);
            return clone.ToString();
        }

        public string Format(QueryExpression expression)
        {
            var clone = Clone(expression);
            clearAddedMetadataVisitor.Visit(clone);
            return clone.ToString();
        }

        private T Clone<T>(T obj)
            => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj))!;
    }
}
