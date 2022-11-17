//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from c:\Projects\Packer\Packer2.Library\MinifiedQueryParser\pbiqParser.g4 by ANTLR 4.9.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Packer2 {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="pbiqParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.2")]
[System.CLSCompliant(false)]
public interface IpbiqParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.root"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRoot([NotNull] pbiqParser.RootContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.query"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitQuery([NotNull] pbiqParser.QueryContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.from"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFrom([NotNull] pbiqParser.FromContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.fromElement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFromElement([NotNull] pbiqParser.FromElementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.entity_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEntity_name([NotNull] pbiqParser.Entity_nameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.schema"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSchema([NotNull] pbiqParser.SchemaContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.expressionContainer"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionContainer([NotNull] pbiqParser.ExpressionContainerContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.alias"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAlias([NotNull] pbiqParser.AliasContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.where"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhere([NotNull] pbiqParser.WhereContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.queryFilterElement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitQueryFilterElement([NotNull] pbiqParser.QueryFilterElementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.transform"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTransform([NotNull] pbiqParser.TransformContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.parameters"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameters([NotNull] pbiqParser.ParametersContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.parameter"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter([NotNull] pbiqParser.ParameterContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.inputTable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInputTable([NotNull] pbiqParser.InputTableContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.outputTable"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOutputTable([NotNull] pbiqParser.OutputTableContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.tableColumn"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTableColumn([NotNull] pbiqParser.TableColumnContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.algorithm"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAlgorithm([NotNull] pbiqParser.AlgorithmContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.orderby"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOrderby([NotNull] pbiqParser.OrderbyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.orderbySection"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitOrderbySection([NotNull] pbiqParser.OrderbySectionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.direction"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDirection([NotNull] pbiqParser.DirectionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.groupby"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGroupby([NotNull] pbiqParser.GroupbyContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.skip"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSkip([NotNull] pbiqParser.SkipContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.top"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTop([NotNull] pbiqParser.TopContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.select"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSelect([NotNull] pbiqParser.SelectContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression([NotNull] pbiqParser.ExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.containsExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitContainsExpr([NotNull] pbiqParser.ContainsExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.betweenExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBetweenExpr([NotNull] pbiqParser.BetweenExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.inExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInExpr([NotNull] pbiqParser.InExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.compareExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompareExpr([NotNull] pbiqParser.CompareExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.primary_expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrimary_expression([NotNull] pbiqParser.Primary_expressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.propertyExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPropertyExpression([NotNull] pbiqParser.PropertyExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.subQueryExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSubQueryExpr([NotNull] pbiqParser.SubQueryExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.sourceRefExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSourceRefExpr([NotNull] pbiqParser.SourceRefExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.aggregationExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAggregationExpr([NotNull] pbiqParser.AggregationExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.anyValueExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAnyValueExpr([NotNull] pbiqParser.AnyValueExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.nullEpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNullEpr([NotNull] pbiqParser.NullEprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.intExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIntExpr([NotNull] pbiqParser.IntExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.datetimeExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDatetimeExpr([NotNull] pbiqParser.DatetimeExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.dateExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDateExpr([NotNull] pbiqParser.DateExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.hierarchySource"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitHierarchySource([NotNull] pbiqParser.HierarchySourceContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.hierarchyExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitHierarchyExpr([NotNull] pbiqParser.HierarchyExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.hierarchyLevelExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitHierarchyLevelExpr([NotNull] pbiqParser.HierarchyLevelExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.variationExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariationExpr([NotNull] pbiqParser.VariationExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.datetimeSecExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDatetimeSecExpr([NotNull] pbiqParser.DatetimeSecExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.dateSpanExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDateSpanExpr([NotNull] pbiqParser.DateSpanExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.boolExp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolExp([NotNull] pbiqParser.BoolExpContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.notExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNotExpr([NotNull] pbiqParser.NotExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.scopedEvalExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitScopedEvalExpr([NotNull] pbiqParser.ScopedEvalExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.encodedLiteralExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEncodedLiteralExpr([NotNull] pbiqParser.EncodedLiteralExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.inExprValues"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInExprValues([NotNull] pbiqParser.InExprValuesContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.inExprEqualityKind"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInExprEqualityKind([NotNull] pbiqParser.InExprEqualityKindContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.roleRefExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRoleRefExpression([NotNull] pbiqParser.RoleRefExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.expressionOrExpressionList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionOrExpressionList([NotNull] pbiqParser.ExpressionOrExpressionListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.arithmenticExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArithmenticExpr([NotNull] pbiqParser.ArithmenticExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.logicalExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicalExpr([NotNull] pbiqParser.LogicalExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.transformOutputRoleRefExpr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTransformOutputRoleRefExpr([NotNull] pbiqParser.TransformOutputRoleRefExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.binary_arithmetic_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBinary_arithmetic_operator([NotNull] pbiqParser.Binary_arithmetic_operatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.binary_logic_operator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBinary_logic_operator([NotNull] pbiqParser.Binary_logic_operatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.timeUnit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTimeUnit([NotNull] pbiqParser.TimeUnitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.left"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLeft([NotNull] pbiqParser.LeftContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.right"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRight([NotNull] pbiqParser.RightContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.comparisonOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitComparisonOperator([NotNull] pbiqParser.ComparisonOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="pbiqParser.identifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentifier([NotNull] pbiqParser.IdentifierContext context);
}
} // namespace Packer2
