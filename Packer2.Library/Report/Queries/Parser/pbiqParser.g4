parser grammar pbiqParser;

options { tokenVocab = pbiqLexer; }

queryRoot: query EOF;
expressionRoot: expression EOF;

query: /*parameters*/ /*let*/ from where? transform* orderby? select? /*visualshape*/ groupby? skip? top?;

from: FROM fromElement (COMMA fromElement)*;
fromElement: alias IN (entity_name | subQueryExpr);
entity_name: (schema DOT)? identifier;
schema: identifier;
expressionContainer: expression (AS alias)? (WITH NATIVEREFERENCENAME)?;
alias: identifier;

where: WHERE whereCriterion (COMMA whereCriterion)*;
// todo: should we limit this to only expressions that return a boolean?
whereCriterion: /*target*/ expression /*annotations, filter, restatement*/;

transform: TRANSFORM VIA transform_algorithm AS identifier WITH transform_parameters? COMMA? transform_inputTable? COMMA? transform_outputTable?;
transform_parameters: INPUTPARAMETERS LPAREN transform_parameter (COMMA transform_parameter)* RPAREN;
transform_parameter: expression (AS alias)?;
transform_inputTable: INPUTTABLE LPAREN transform_tableColumn (COMMA transform_tableColumn)* RPAREN AS alias;
transform_outputTable: OUTPUTTABLE LPAREN transform_tableColumn (COMMA transform_tableColumn)* RPAREN AS alias;
transform_tableColumn: expression (AS alias)? (WITH ROLE STRING_LITERAL)?;
transform_algorithm: STRING_LITERAL;

orderby: ORDERBY orderingCriterion (COMMA orderingCriterion)*;
orderingCriterion: expression direction;
direction: ASCENDING | DESCENDING;

groupby: GROUPBY expression (COMMA expression)*;

skip: SKIP_ INTEGER;
top: TOP INTEGER;

select: SELECT expression (COMMA expression)*;

/* Expressions */

expression
	: primary_expression
	| containsExpr
	| betweenExpr
	| inExpr
	| compareExpr
	| binaryStringExpr
	| indexer
	;

containsExpr: primary_expression CONTAINS right;
betweenExpr: primary_expression BETWEEN left AND right;
inExpr: primary_expression IN (sourceRefExpr | inExprValues) | LPAREN expression (COMMA expression) RPAREN IN (sourceRefExpr | inExprValues);
compareExpr: primary_expression comparisonOperator right;
binaryStringExpr: primary_expression binary_string_operator right;
binary_string_operator: STARTSWITH | ENDSWITH;
indexer: IDENTIFIER QUOTED_IDENTIFIER;
// todo: split up expressions into levels where each level can only reference levels below?
// could a level ever have to reference itself? i.e. could the first rule in the expression refere to the same level?
//   if so, this is fine, as long we don't care about priority between the rules in the same level.

primary_expression: 
	LPAREN expression RPAREN
	| funcExpr
	| variationExpr
	| anyValueExpr
	| arithmenticExpr
	| datetimeSecExpr
	| encodedLiteralExpr
	| hierarchyExpr
	| hierarchyLevelExpr
	| logicalExpr
	| transformOutputRoleRefExpr
	| propertyExpression
	| scopedEvalExpr
	| sourceRefExpr
	| subQueryExpr
	| defaultValueExpr
	;

propertyExpression: (sourceRefExpr | subQueryExpr) DOT identifier;

subQueryExpr: LCURLY query RCURLY;
sourceRefExpr: identifier;
funcExpr: identifier LPAREN (arg (COMMA arg)*)? RPAREN;
arg: expression | number | IDENTIFIER;
number: INTEGER | DECIMAL;
anyValueExpr: ANYVALUE WITH DEFAULTVALUEOVERRIDESANCESTORS;
/*tmp: could this be "expression" instead of sourceRefExpr? avoiding indirect left recursion*/
hierarchySource: sourceRefExpr | variationExpr;
hierarchyExpr: hierarchySource DOT HIERARCHY LPAREN identifier RPAREN;
hierarchyLevelExpr: hierarchyExpr DOT LEVEL LPAREN identifier RPAREN;
/*tmp: could this be "expression" instead of sourceRefExpr? avoiding indirect left recursion*/
variationExpr: sourceRefExpr DOT VARIATION LPAREN identifier COMMA identifier RPAREN;
datetimeSecExpr: DATETIME;
// boolExp: TRUE | FALSE;
scopedEvalExpr: SCOPEDEVAL LPAREN expression COMMA SCOPE LPAREN (expression (COMMA expression)*)? RPAREN RPAREN;
encodedLiteralExpr: STRING_LITERAL | INTEGER_LITERAL | DECIMAL_LITERAL | DOUBLE_LITERAL | BASE64BYTES_LITERAL | DATEIME_LITERAL | TRUE | FALSE | NULL;
inExprValues: LPAREN expressionOrExpressionList (COMMA expressionOrExpressionList)* RPAREN (USING inExprEqualityKind)?;
inExprEqualityKind: identifier;
expressionOrExpressionList: expression | LPAREN expression (COMMA expression)+ RPAREN;
arithmenticExpr: LPAREN left binary_arithmetic_operator right RPAREN;
logicalExpr: LPAREN left binary_logic_operator right RPAREN;
transformOutputRoleRefExpr: TRANSFORMOUTPUTROLE LPAREN STRING_LITERAL RPAREN;
binary_arithmetic_operator: PLUS | MINUS | DIV | MULT;
binary_logic_operator: AND | OR;
amount: INTEGER;
timeunit: identifier;
defaultValueExpr: DEFAULTVALUE; 

timeUnit: IDENTIFIER;
left: expression;
right: expression;
comparisonOperator: GT | LT | EQ | GTE | LTE;

identifier
	: IDENTIFIER 
	| QUOTED_IDENTIFIER 
	| LEVEL
	| VARIATION
	/* todo: all keywords can show up as identifiers as well unfortunately so add them here */;
	
// According to V1ToV2 class in the infonav dll (derives from QueryDefinitionUpgrader), 
// we do not need the following expressions:

/*
Literal:
	QueryNullConstantExpression,
	QueryStringConstantExpression,
	QueryIntegerConstantExpression,
	QueryDecimalConstantExpression,
	QueryNumberConstantExpression,
	QueryBooleanConstantExpression,
	QueryDateTimeConstantExpression
DateSpan:
	QueryDateConstantExpression, (* - see method Visit(QueryDateConstantExpression expression))
	QueryDateTimeSecondConstantExpression,
	QueryYearConstantExpression
	QueryYearAndMonthConstantExpression
	QueryYearAndWeekConstantExpression
	QueryDecadeConstantExpression
	QueryDatePartExpression
 */