parser grammar pbiqParser;

options { tokenVocab = pbiqLexer; }

root: query EOF;
expressionRoot: expression EOF;

query: /*parameters*/ /*let*/ from where? transform* orderby? select? /*visualshape*/ groupby? skip? top?;

from: FROM fromElement (COMMA fromElement)*;
fromElement: alias IN (entity_name | subQueryExpr);
entity_name: (schema DOT)? identifier;
schema: identifier;
expressionContainer: expression (AS alias)? (WITH NATIVEREFERENCENAME)?;
alias: identifier;

where: WHERE queryFilterElement (COMMA queryFilterElement)*;
queryFilterElement: /*target*/ expression /*annotations, filter, restatement*/;

transform: TRANSFORM VIA algorithm AS identifier WITH parameters? COMMA? inputTable? COMMA? outputTable?;
parameters: INPUTPARAMETERS LPAREN parameter (COMMA parameter)* RPAREN;
parameter: expression (AS alias)?;
inputTable: INPUTTABLE LPAREN tableColumn (COMMA tableColumn)* RPAREN AS alias;
outputTable: OUTPUTTABLE LPAREN tableColumn (COMMA tableColumn)* RPAREN AS alias;
tableColumn: expression (AS alias)? (WITH ROLE STRING_LITERAL)?;
algorithm: STRING_LITERAL;

// transform: TRANSFORM VIA STRING_LITERAL AS alias WITH --todo;

orderby: ORDERBY orderbySection (COMMA orderbySection)*;
orderbySection: expression direction;
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
	;

containsExpr: primary_expression CONTAINS right;
betweenExpr: primary_expression BETWEEN left AND right;
inExpr: primary_expression IN (sourceRefExpr | inExprValues) | LPAREN expression (COMMA expression) RPAREN IN (sourceRefExpr | inExprValues);
compareExpr: primary_expression comparisonOperator right;

// todo: split up expressions into levels where each level can only reference levels below?
// could a level ever have to reference itself? i.e. could the first rule in the expression refere to the same level?
//   if so, this is fine, as long we don't care about priority between the rules in the same level.

primary_expression: 
	LPAREN expression RPAREN
	| aggregationExpr
	| variationExpr
	| anyValueExpr
	| arithmenticExpr
//	| boolExp
	| dateExpr
	| datetimeExpr
	| datetimeSecExpr
	| dateSpanExpr
	| encodedLiteralExpr
	| hierarchyExpr
	| hierarchyLevelExpr
	| intExpr
	| logicalExpr
	| notExpr
//	| nullEpr
	| transformOutputRoleRefExpr
	| roleRefExpression
	| propertyExpression
	| scopedEvalExpr
	| sourceRefExpr
	| subQueryExpr
	;

propertyExpression: (sourceRefExpr | subQueryExpr) DOT identifier;

subQueryExpr: LCURLY query RCURLY;
sourceRefExpr: identifier;
aggregationExpr: identifier LPAREN expression RPAREN;
anyValueExpr: ANYVALUE WITH DEFAULTVALUEOVERRIDESANCESTORS;
// nullEpr: NULL;
intExpr: INTEGER;
datetimeExpr: DATETIME;
dateExpr: DATE;
/*tmp: could this be "expression" instead of sourceRefExpr? avoiding indirect left recursion*/
hierarchySource: sourceRefExpr | variationExpr;
hierarchyExpr: hierarchySource DOT HIERARCHY LPAREN identifier RPAREN;
hierarchyLevelExpr: hierarchyExpr DOT LEVEL LPAREN identifier RPAREN;
/*tmp: could this be "expression" instead of sourceRefExpr? avoiding indirect left recursion*/
variationExpr: sourceRefExpr DOT VARIATION LPAREN identifier COMMA identifier RPAREN;
datetimeSecExpr: DATETIME;
dateSpanExpr: DATESPAN LPAREN timeUnit COMMA expression RPAREN;
// boolExp: TRUE | FALSE;
notExpr: NOT LPAREN expression RPAREN;
scopedEvalExpr: SCOPEDEVAL LPAREN expression COMMA SCOPE LPAREN (expression (COMMA expression)*)? RPAREN RPAREN;
encodedLiteralExpr: STRING_LITERAL | INTEGER_LITERAL | DECIMAL_LITERAL | DOUBLE_LITERAL | BASE64BYTES_LITERAL | DATEIME_LITERAL | TRUE | FALSE | NULL;
inExprValues: LPAREN expressionOrExpressionList (COMMA expressionOrExpressionList)* RPAREN (USING inExprEqualityKind)?;
inExprEqualityKind: identifier;
roleRefExpression: ROLEREF QUOTED_IDENTIFIER;
expressionOrExpressionList: expression | LPAREN expression (COMMA expression)+ RPAREN;
arithmenticExpr: LPAREN left binary_arithmetic_operator right RPAREN;
logicalExpr: LPAREN left binary_logic_operator right RPAREN;
transformOutputRoleRefExpr: TRANSFORMOUTPUTROLE LPAREN STRING_LITERAL RPAREN;

binary_arithmetic_operator: PLUS | MINUS | DIV | MULT;
binary_logic_operator: AND | OR;

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