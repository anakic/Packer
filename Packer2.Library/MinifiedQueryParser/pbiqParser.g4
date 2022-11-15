parser grammar pbiqParser;

options {
	tokenVocab = pbiqLexer;
}

root: query EOF;

query: /*parameters*/ /*let*/ from where? /*transform?*/ orderby? select? /*visualshape*/ groupby? skip? top?;

from: FROM fromElement (COMMA fromElement)*;
fromElement: alias IN (entity | expressionContainer);
expressionContainer: expression (AS alias)? (WITH NATIVEREFERENCENAME)?;

where: WHERE queryFilterElement (COMMA queryFilterElement)*;
queryFilterElement:
	/* missing "Target" segment*/ filterExpression /*missing annotations and filter restatement*/;

alias: IDENTIFIER;

entity: (schema DOT)? entity_name;
schema: IDENTIFIER;
entity_name: IDENTIFIER;

// transform: TRANSFORM VIA STRING_LITERAL AS alias WITH --todo;
orderby: ORDERBY orderbySection (COMMA orderbySection)*;
orderbySection: expression direction;
direction: ASCENDING | DESCENDING;

groupby: ORDERBY expression (COMMA expression)*;

skip: SKIP_ INTEGER;
top: TOP INTEGER;

select: SELECT expression (COMMA expression)*;

/* Expressions */

expression: filterExpression | nonFilterExpression;

nonFilterExpression:
	aggregationExpr
	| arithmenticExpr
	| anyValueExpr
	| literalExpr
	| nullEpr
	| intExpr
	| decimalExpr
	| stringExpr
	| datetimeExpr
	| datetimeSecExpr
	| dateSpanExpr
	| scopedEvalExpr
	| dateExpr
	| sourceRefExpr
	| subQueryExpr
	| hierarchyExpr
	| hierarchyLevelExpr
	| LPAREN nonFilterExpression RPAREN
	| nonFilterExpression propertyExpression_seg
	;

filterExpression:
	| andExpr
	| orExpr
	| nonLeftRecursiveFilterExpression
	;

nonLeftRecursiveFilterExpression: 
	notExpr
	| betweenExpr 
	| boolExp
	| inExpr
	| comparisonExpr
	| containsExpr
	| LPAREN filterExpression RPAREN
	;

subQueryExpr: LCURLY query RCURLY;
sourceRefExpr: IDENTIFIER;
aggregationExpr: IDENTIFIER LPAREN expression RPAREN;
anyValueExpr: ANYVALUE WITH DEFAULTVALUEOVERRIDESANCESTORS;
andExpr: left AND right;
orExpr: left OR right;
betweenExpr: nonFilterExpression BETWEEN first AND second;
nullEpr: NULL;
intExpr: INTEGER;
decimalExpr: DECIMAL_LITERAL;
datetimeExpr: DATETIME;
dateExpr: DATE;
/*tmp: should be expression instead of sourceRefExpr but avoiding indirect left recursion*/
hierarchyExpr: sourceRefExpr DOT HIERARCHY LPAREN IDENTIFIER RPAREN;
hierarchyLevelExpr: hierarchyExpr DOT LEVEL LPAREN IDENTIFIER RPAREN;
datetimeSecExpr: DATETIMESECOND;
dateSpanExpr: DATESPAN LPAREN timeUnit COMMA expression RPAREN;
containsExpr: first CONTAINS second;
stringExpr: STRING_LITERAL;
boolExp: TRUE | FALSE;
comparisonExpr: first operator second;
propertyExpression_seg: DOT IDENTIFIER;
notExpr: NOT LPAREN expression RPAREN; 
scopedEvalExpr: SCOPEDEVAL LPAREN expression COMMA SCOPE LPAREN RPAREN (expression (COMMA expression)+)? RPAREN;

literalExpr: STRING_LITERAL | INTEGER_LITERAL | DECIMAL_LITERAL | DOUBLE_LITERAL | BASE64BYTES_LITERAL | DATEIME_LITERAL;
inExpr:
	(nonFilterExpression | LPAREN nonFilterExpression (COMMA nonFilterExpression RPAREN)) 
	IN (tableName | inExprValues);
inExprValues: LPAREN expressionOrExpressionList (COMMA expressionOrExpressionList)* RPAREN (USING equalityKind)?;
expressionOrExpressionList
	: expression | LPAREN expression (COMMA expression)+ RPAREN;
arithmenticExpr:
	LPAREN left BINARY_ARITHMETIC_OPERATOR right RPAREN;

orderByClause: ORDERBY expression;

timeUnit: IDENTIFIER;
tableName: IDENTIFIER;

equalityKind: IDENTIFIER;
left: nonLeftRecursiveFilterExpression;
right: filterExpression;
first: nonFilterExpression;
second: nonFilterExpression;
operator: GT | LT | EQ | GTE | LTE;