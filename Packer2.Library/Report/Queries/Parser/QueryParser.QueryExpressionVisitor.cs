using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.InfoNav;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Packer2.Library.MinifiedQueryParser.QueryTransforms;
using System.Runtime.CompilerServices;

namespace Packer2.Library.Report.QueryTransforms.Antlr
{
    public partial class QueryParser
    {
        class QueryExpressionVisitor : pbiqParserBaseVisitor<QueryExpression>
        {
            private readonly Dictionary<string, string> sourceNames;
            private readonly HashSet<string> transformTableNames;
            private readonly ColumnsAndMeasuresGlossary glossary;
            private ParserResultValidator validator;
            private readonly bool standalone;

            public QueryExpression VisitValidated(IParseTree tree)
            {
                var res = Visit(tree);
                if (res == null)
                    throw new Exception("Failed to resolve query");

                validator.ValidateExpression(res, standalone);

                return res;
            }

            public QueryExpressionVisitor(ColumnsAndMeasuresGlossary glossary, ParserResultValidator validator, bool standalone, Dictionary<string, string> sourceNames, HashSet<string> transformTableNames)
            {
                this.glossary = glossary;
                this.validator = validator;
                this.standalone = standalone;
                this.sourceNames = sourceNames;
                this.transformTableNames = transformTableNames;
            }

            public override QueryExpression VisitArithmenticExpr([NotNull] pbiqParser.ArithmenticExprContext context)
            {
                var left = context.left();
                var right = context.right();
                var opeartor = ParseOperator(context.binary_arithmetic_operator().GetText());
                return new QueryArithmeticExpression()
                {
                    Left = VisitValidated(left),
                    Right = VisitValidated(right),
                    Operator = opeartor
                };
            }

            private QueryArithmeticOperatorKind ParseOperator(string v)
            {
                switch (v)
                {
                    case "-":
                        return QueryArithmeticOperatorKind.Subtract;
                    case "+":
                        return QueryArithmeticOperatorKind.Add;
                    case "*":
                        return QueryArithmeticOperatorKind.Multiply;
                    case "/":
                        return QueryArithmeticOperatorKind.Divide;
                    default:
                        throw new ArgumentException("Invalid operator");
                }
            }

            public override QueryExpression VisitAnyValueExpr([NotNull] pbiqParser.AnyValueExprContext context)
            {
                return new QueryAnyValueExpression();
            }



            public override QueryExpression VisitScopedEvalExpr([NotNull] pbiqParser.ScopedEvalExprContext context)
            {
                var expression = VisitValidated(context.expression()[0]);
                var scopes = context.expression().Skip(1).Select(ctx => (QueryExpressionContainer)VisitValidated(ctx)).ToList();
                return new QueryScopedEvalExpression()
                {
                    Expression = expression,
                    Scope = scopes
                };
            }

            public override QueryExpression VisitVariationExpr([NotNull] pbiqParser.VariationExprContext context)
            {
                var expression = VisitValidated(context.sourceRefExpr());
                if (context.identifier().Count() != 2)
                    throw new FormatException("Variation syntax is: expr.variation(property, name). Expecting two identifiers (property and name)!");
                var property = UnescapeIdentifier(context.identifier().ElementAt(0).GetText());
                var name = UnescapeIdentifier(context.identifier().ElementAt(1).GetText());

                return new QueryPropertyVariationSourceExpression()
                {
                    Expression = expression,
                    Property = property,
                    Name = name
                };
            }

            public override QueryExpression VisitHierarchyExpr([NotNull] pbiqParser.HierarchyExprContext context)
            {
                var hierarchy = UnescapeIdentifier(context.identifier().GetText());
                var expression = VisitValidated(context.hierarchySource());

                return new QueryHierarchyExpression()
                {
                    Hierarchy = hierarchy,
                    Expression = expression
                };
            }

            public override QueryExpression VisitFuncExpr([NotNull] pbiqParser.FuncExprContext context)
            {
                var functionStr = context.identifier().GetText()?.ToLower();

                QueryExpression expression;
                var argReader = new FuncArgReader(context, VisitValidated);

                if (Enum.TryParse<QueryAggregateFunction>(functionStr, true, out var aggFunc))
                    expression = new QueryAggregationExpression()
                    {
                        Expression = argReader.ReadExpr(),
                        Function = aggFunc
                    };
                else if (Enum.TryParse<QueryDatePartFunction>(functionStr, true, out var datePartFunc))
                    expression = new QueryDatePartExpression()
                    {
                        Expression = argReader.ReadExpr(),
                        Function = datePartFunc
                    };
                else if (functionStr == "any")
                    expression = new QueryExistsExpression()
                    {
                        Expression = argReader.ReadExpr()
                    };
                else if (functionStr == "dateadd")
                    expression = new QueryDateAddExpression()
                    {
                        Amount = argReader.ReadInt(),
                        TimeUnit = argReader.ReadEnum<TimeUnit>(),
                        Expression = argReader.ReadExpr()
                    };
                else if (functionStr == "datespan")
                    expression = new QueryDateSpanExpression()
                    {
                        TimeUnit = argReader.ReadEnum<TimeUnit>(),
                        Expression = argReader.ReadExpr()
                    };
                else if (functionStr == "discretize")
                    expression = new QueryDiscretizeExpression()
                    {
                        Expression = argReader.ReadExpr(),
                        Count = argReader.ReadInt()
                    };
                else if (functionStr == "floor")
                    expression = new QueryFloorExpression()
                    {
                        Expression = argReader.ReadExpr(),
                        Size = argReader.ReadDouble(),
                        TimeUnit = argReader.ReadArgAsEnumIfExists<TimeUnit>()
                    };
                else if (functionStr == "member")
                    expression = new QueryMemberExpression()
                    {
                        Expression = argReader.ReadExpr(),
                        Member = argReader.ReadString()
                    };
                else if (functionStr == "nativeformat")
                    expression = new QueryNativeFormatExpression()
                    {
                        Expression = argReader.ReadExpr(),
                        FormatString = argReader.ReadString()
                    };
                else if (functionStr == "nativemeasure")
                    expression = new QueryNativeMeasureExpression()
                    {
                        Language = argReader.ReadString(),
                        Expression = argReader.ReadString(),
                    };
                else if (functionStr == "not")
                    expression = new QueryNotExpression()
                    {
                        Expression = argReader.ReadExpr(),
                    };
                else if (functionStr == "now")
                    expression = new QueryNowExpression();
                else if (functionStr == "percentile")
                    expression = new QueryPercentileExpression()
                    {
                        Exclusive = argReader.ReadString() == "exclusive" ? true : false,
                        K = argReader.ReadDouble(),
                        Expression = argReader.ReadExpr(),
                    };
                else if (functionStr == "tabletype")
                    expression = new QueryTableTypeExpression()
                    {
                        Columns = argReader.ReadExprs().Select(e => (QueryExpressionContainer)e).ToList(),
                    };
                else if (functionStr == "transformoutputrole")
                    expression = new QueryTransformOutputRoleRefExpression()
                    {
                        Role = argReader.ReadString()
                    };
                else if (functionStr == "typeof")
                    expression = new QueryTypeOfExpression()
                    {
                        Expression = argReader.ReadExpr()
                    };
                else if (functionStr == "nativevisualcalculation")
                    expression = new QueryNativeVisualCalculationExpression()
                    {
                        Language = argReader.ReadString(),
                        Expression = argReader.ReadString()
                    };
                else
                    throw new NotImplementedException($"Not expecting function '{functionStr}'. Todo: send this error message and the pbix to Packer2 maintainer/s.");

                argReader.EnsureNoRemainingArgs();
                return expression;
            }

            private class FuncArgReader
            {
                int argIdx;
                int argsCount;
                private readonly pbiqParser.FuncExprContext context;
                private readonly Func<IParseTree, QueryExpression> parseExprFunc;

                public FuncArgReader(pbiqParser.FuncExprContext context, Func<IParseTree, QueryExpression> parseExprFunc)
                {
                    this.context = context;
                    this.parseExprFunc = parseExprFunc;
                    argsCount = context.arg().Count();
                }

                public ICollection<QueryExpression> ReadExprs()
                {
                    return context.arg().Select(parseExprFunc).ToList();
                }

                public QueryExpression ReadExpr()
                {
                    return parseExprFunc(context.arg().ElementAt(argIdx++));
                }

                public int ReadInt()
                {
                    return int.Parse(ReadString());
                }

                public double ReadDouble()
                {
                    return double.Parse(ReadString());
                }

                public string ReadString()
                {
                    return context.arg().ElementAt(argIdx++).GetText();
                }

                public T ReadEnum<T>() where T : struct
                {
                    var str = ReadString();
                    return Enum.Parse<T>(str, true);
                }

                public T? ReadArgAsEnumIfExists<T>() where T : struct
                {
                    if (argsCount <= argIdx)
                        return null;

                    return ReadEnum<T>();
                }

                public void EnsureNoRemainingArgs()
                {
                    if (argIdx < argsCount)
                        throw new FormatException($"Function '{context.identifier().GetText()}' got {argsCount} arguments but it only accepts {argIdx}");
                }
            }

            private string ReadStringLiteral(ITerminalNode node)
            {
                var text = node.GetText();
                return text.Substring(1, text.Length - 2);
            }

            public override QueryExpression VisitTransformOutputRoleRefExpr([NotNull] pbiqParser.TransformOutputRoleRefExprContext context)
            {
                return new QueryTransformOutputRoleRefExpression() { Role = ReadStringLiteral(context.STRING_LITERAL()) };
            }

            public override QueryExpression VisitSubQueryExpr([NotNull] pbiqParser.SubQueryExprContext context)
            {
                var queryDefVisitor = new QueryConstructorVisitor(glossary, validator);
                var query = queryDefVisitor.Visit(context.query());

                return new QuerySubqueryExpression() { Query = query };
            }

            public override QueryExpression VisitHierarchyLevelExpr([NotNull] pbiqParser.HierarchyLevelExprContext context)
            {
                var expression = VisitValidated(context.hierarchyExpr());
                var level = UnescapeIdentifier(context.identifier().GetText());
                return new QueryHierarchyLevelExpression()
                {
                    Expression = expression,
                    Level = level
                };
            }

            public override QueryExpression VisitFilteredEvalExpr([NotNull] pbiqParser.FilteredEvalExprContext context)
            {
                // todo: Check if this works.
                // I never checked if this works properly because I did not have a pbix file that uses this kind of expressions
                return new QueryFilteredEvalExpression()
                {
                    Expression = VisitValidated(context.expression()),
                    Filters = context.whereCriterion().Select(qf => new QueryFilter() { Condition = VisitValidated(qf) }).ToList()
                };
            }

            public override QueryExpression VisitSparkLineDataExpr([NotNull] pbiqParser.SparkLineDataExprContext context)
            {
                // todo: Check if this works.
                // I never checked if this works properly because I did not have a pbix file that uses this kind of expressions
                return new QuerySparklineDataExpression()
                {
                    Measure = VisitValidated(context.sparkLineDataMeasure()),
                    Groupings = context.expression().Select(VisitValidated).Select(ex => (QueryExpressionContainer)ex).ToList(),
                    PointsPerSparkline = int.Parse(context.INTEGER().GetText()),
                    IncludeMinGroupingInterval = context.INCLUDEMINGROUPINGINTERVAL() != null,
                    ScalarKey = context.scalarKey() != null ? VisitValidated(context.scalarKey()) : null
                };
            }

            public override QueryExpression VisitInExpr([NotNull] pbiqParser.InExprContext context)
            {
                var expression = new QueryInExpression();
                if (context.sourceRefExpr() != null)
                    expression.Table = new QueryExpressionContainer(VisitValidated(context.sourceRefExpr()));
                else
                {
                    if (context.inExprValues().inExprEqualityKind() != null)
                        expression.EqualityKind = context.inExprValues().inExprEqualityKind().identifier().GetText().ToLower() == "identity" ? QueryEqualitySemanticsKind.Identity : QueryEqualitySemanticsKind.Equality;

                    expression.Values = context.inExprValues()
                        .expressionOrExpressionList()
                        .Select(el => el.expression().Select(exp => new QueryExpressionContainer(VisitValidated(exp))).ToList()).ToList();
                }

                if (context.primary_expression() != null)
                    expression.Expressions = new List<QueryExpressionContainer>() { VisitValidated(context.primary_expression()) };
                else
                    expression.Expressions = context.expression().Select(exp => new QueryExpressionContainer(VisitValidated(exp))).ToList();
                return expression;
            }

            public override QueryExpression VisitSourceRefExpr([NotNull] pbiqParser.SourceRefExprContext context)
            {
                var name = UnescapeIdentifier(context.identifier().GetText());

                // the identifier might be a QueryPrimitiveTypeExpression value (enum) or a reference to a source, we don't know ahead of time.
                // todo: might want to reconsider the naming of this node.
                if (Enum.TryParse<ConceptualPrimitiveType>(name, out var val))
                {
                    return new QueryPrimitiveTypeExpression() { Type = val };
                }
                

                if (transformTableNames.Contains(name))
                {
                    return new QueryTransformTableRefExpression() { Source = name };
                }
                else
                {
                    // todo: what is the schema? the schema is not saved to the minified query.
                    var expression = new QuerySourceRefExpression();
                    if (sourceNames.ContainsKey(name))
                        expression.Source = name;
                    else
                    {
                        // todo: if we do add a reference to the model (to distinguish between column and measure properties), we should check that the entity exists
                        expression.Entity = name;
                    }
                    return expression;
                }
            }

            public override QueryExpression VisitIndexer([NotNull] pbiqParser.IndexerContext context)
            {
                var left = context.IDENTIFIER().GetText().ToLower();
                var arg = UnescapeIdentifier(context.QUOTED_IDENTIFIER().GetText());
                if (left == "letref")
                    return new QueryLetRefExpression() { Name = arg };
                if (left == "parameterref")
                    return new QueryParameterRefExpression() { Name = arg };
                if (left == "roleRef")
                    return new QueryRoleRefExpression() { Role = arg };
                if (left == "summaryvalueref")
                    return new QuerySummaryValueRefExpression() { Name = arg };
                else
                    throw new NotImplementedException("Todo: expression currently not supported. Please send this error message and the pbix/t file that caused it to maintainer of Packer.");
            }

            public override QueryExpression VisitPropertyExpression([NotNull] pbiqParser.PropertyExpressionContext context)
            {
                var expressionContainer = (QueryExpressionContainer)VisitValidated((ParserRuleContext)context.sourceRefExpr() ?? context.subQueryExpr());
                var property = UnescapeIdentifier(context.identifier().GetText());

                if (expressionContainer.TransformTableRef != null)
                {
                    if (!transformTableNames.Contains(expressionContainer.TransformTableRef.Source))
                        throw new Exception("Invalid output transform name");
                    return new QueryColumnExpression()
                    {
                        Expression = expressionContainer,
                        Property = property
                    };
                }

                string entity = null;
                if (expressionContainer.SourceRef != null)
                    entity = expressionContainer.SourceRef.Entity ?? sourceNames[expressionContainer.SourceRef.Source];

                if (entity != null && glossary.IsMeasure(entity, property))
                {
                    return new QueryMeasureExpression()
                    {
                        Expression = expressionContainer,
                        Property = property
                    };
                }
                else if (entity != null && glossary.IsColumn(entity, property))
                {
                    return new QueryColumnExpression()
                    {
                        Expression = expressionContainer,
                        Property = property
                    };
                }
                else
                {
                    if (expressionContainer.Expression is QuerySubqueryExpression se)
                    {
                        var selectExpr = se.Query.Select.Select(x => (QueryPropertyExpression)x.Expression).SingleOrDefault(ec =>
                        {
                            var sourceNames = se.Query.From.ToDictionary(f => f.Name, f => f.Entity) ?? new Dictionary<string, string>();
                            if (se.Query.Transform != null)
                            {
                                se.Query.Transform.Select(t => t.Input?.Table.Name).Where(x => x != null).ToList().ForEach(t => sourceNames.Add(t, t));
                                se.Query.Transform.Select(t => t.Output?.Table.Name).Where(x => x != null).ToList().ForEach(t => sourceNames.Add(t, t));
                            }

                            if (ec.Expression.SourceRef != null)
                            {
                                var sourceRef = ec.Expression.SourceRef;
                                var entityName = sourceRef.Entity ?? sourceNames[sourceRef.Source];
                                return $"{entityName}.{ec.Property}" == property;
                            }
                            else if (ec.Expression.TransformTableRef != null)
                            {
                                return ec.Property == property;
                            }
                            else
                                throw new NotImplementedException("Unexpected element in subquery's select list");
                        });

                        if (selectExpr is QueryColumnExpression)
                        {
                            return new QueryColumnExpression()
                            {
                                Expression = expressionContainer,
                                Property = property
                            };
                        }
                        else if (selectExpr is QueryMeasureExpression)
                        {
                            return new QueryMeasureExpression()
                            {
                                Expression = expressionContainer,
                                Property = property
                            };
                        }
                    }

                    return new QueryPropertyExpression()
                    {
                        Expression = expressionContainer,
                        Property = property
                    };
                }
            }


            public override QueryExpression VisitEncodedLiteralExpr([NotNull] pbiqParser.EncodedLiteralExprContext context)
            {
                return new QueryLiteralExpression() { Value = context.GetText() };
            }

            public override QueryExpression VisitBetweenExpr([NotNull] pbiqParser.BetweenExprContext context)
            {
                return new QueryBetweenExpression()
                {
                    Expression = VisitValidated(context.primary_expression()),
                    LowerBound = VisitValidated(context.left()),
                    UpperBound = VisitValidated(context.right())
                };
            }

            public override QueryExpression VisitCompareExpr([NotNull] pbiqParser.CompareExprContext context)
            {
                var left = VisitValidated(context.primary_expression());
                var right = VisitValidated(context.right());

                return new QueryComparisonExpression()
                {
                    ComparisonKind = GetComparisonKind(context.comparisonOperator()),
                    Left = left,
                    Right = right,
                };
            }

            public override QueryExpression VisitDefaultValueExpr([NotNull] pbiqParser.DefaultValueExprContext context)
            {
                return new QueryDefaultValueExpression();
            }

            private QueryComparisonKind GetComparisonKind(pbiqParser.ComparisonOperatorContext operatorContext)
            {
                if (operatorContext.GT() != null)
                    return QueryComparisonKind.GreaterThan;
                else if (operatorContext.GTE() != null)
                    return QueryComparisonKind.GreaterThanOrEqual;
                if (operatorContext.LT() != null)
                    return QueryComparisonKind.LessThan;
                else if (operatorContext.LTE() != null)
                    return QueryComparisonKind.LessThanOrEqual;
                else
                    return QueryComparisonKind.Equal;
            }

            public override QueryExpression VisitBinaryStringExpr([NotNull] pbiqParser.BinaryStringExprContext context)
            {
                var op = context.binary_string_operator().GetText();
                switch (op)
                {
                    case "startswith":
                        return new QueryStartsWithExpression()
                        {
                            Left = VisitValidated(context.primary_expression()),
                            Right = VisitValidated(context.right())
                        };
                    case "endswith":
                        return new QueryEndsWithExpression()
                        {
                            Left = VisitValidated(context.primary_expression()),
                            Right = VisitValidated(context.right())
                        };
                    default:
                        throw new NotImplementedException($"Not expecting binary operator '{op}'. Todo: send this error message and the pbix to Packer2 maintainer/s.");
                }
                
            }

            public override QueryExpression VisitContainsExpr([NotNull] pbiqParser.ContainsExprContext context)
            {
                return new QueryContainsExpression()
                {
                    Left = VisitValidated(context.primary_expression()),
                    Right = VisitValidated(context.right())
                };
            }

            protected override QueryExpression AggregateResult(QueryExpression aggregate, QueryExpression nextResult)
            {
                return nextResult ?? aggregate;
            }

            public override QueryExpression VisitLogicalExpr([NotNull] pbiqParser.LogicalExprContext context)
            {
                var left = VisitValidated(context.left());
                var right = VisitValidated(context.right());

                if (context.binary_logic_operator().GetText().ToLower() == "and")
                    return new QueryAndExpression() { Left = left, Right = right };
                else if (context.binary_logic_operator().GetText().ToLower() == "or")
                    return new QueryOrExpression() { Left = left, Right = right };
                else
                    throw new FormatException("Invalid logical operator");
            }
        }

        // todo: filteredeval
    }
}
