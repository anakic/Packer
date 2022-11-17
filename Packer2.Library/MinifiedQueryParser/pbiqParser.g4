parser grammar pbiqParser;

options { tokenVocab = pbiqLexer; }

root: query EOF;

query: /*parameters*/ /*let*/ from where? /*transform?*/ orderby? select? /*visualshape*/ groupby? skip? top?;

from: FROM fromElement (COMMA fromElement)*;
fromElement: alias IN (entity_name | subQueryExpr);
entity_name: (schema DOT)? identifier;
schema: identifier;
expressionContainer: expression (AS alias)? (WITH NATIVEREFERENCENAME)?;
alias: identifier;

where: WHERE queryFilterElement (COMMA queryFilterElement)*;
queryFilterElement: /*target*/ expression /*annotations, filter, restatement*/;

// transform: TRANSFORM VIA STRING_LITERAL AS alias WITH --todo;

orderby: ORDERBY orderbySection (COMMA orderbySection)*;
orderbySection: expression direction;
direction: ASCENDING | DESCENDING;

groupby: ORDERBY expression (COMMA expression)*;

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

primary_expression: 
	LPAREN expression RPAREN
	| aggregationExpr
	| anyValueExpr
	| arithmenticExpr
	| boolExp
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
	| nullEpr
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
nullEpr: NULL;
intExpr: INTEGER;
datetimeExpr: DATETIME;
dateExpr: DATE;
/*tmp: should be expression instead of sourceRefExpr but avoiding indirect left recursion*/
hierarchyExpr: sourceRefExpr DOT HIERARCHY LPAREN identifier RPAREN;
hierarchyLevelExpr: hierarchyExpr DOT LEVEL LPAREN identifier RPAREN;
datetimeSecExpr: DATETIMESECOND;
dateSpanExpr: DATESPAN LPAREN timeUnit COMMA expression RPAREN;
boolExp: TRUE | FALSE;
notExpr: NOT LPAREN expression RPAREN; 
scopedEvalExpr: SCOPEDEVAL LPAREN expression COMMA SCOPE LPAREN (expression (COMMA expression)*)? RPAREN;
encodedLiteralExpr: STRING_LITERAL | INTEGER_LITERAL | DECIMAL_LITERAL | DOUBLE_LITERAL | BASE64BYTES_LITERAL | DATEIME_LITERAL;
inExprValues: LPAREN expressionOrExpressionList (COMMA expressionOrExpressionList)* RPAREN (USING inExprEqualityKind)?;
inExprEqualityKind: identifier;
roleRefExpression: ROLEREF QUOTED_IDENTIFIER;
expressionOrExpressionList: expression | LPAREN expression (COMMA expression)+ RPAREN;
arithmenticExpr: LPAREN left binary_arithmetic_operator right RPAREN;
logicalExpr: LPAREN left binary_logic_operator right RPAREN;

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
	/* todo: all keywords can show up as identifiers as well unfortunately so add them here */;