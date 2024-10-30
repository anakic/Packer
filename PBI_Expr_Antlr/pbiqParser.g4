parser grammar pbiqParser;

options {
	tokenVocab = pbiqLexer;
}

query:
	/*parameters*/ /*let*/ from where? /*transform?*/ orderby? select? /*visualshape*/ groupby? skip
		? top? EOF;

from: FROM fromElement (COMMA fromElement)*;
fromElement: alias IN (entity | expressionContainer);
where: WHERE queryFilter (COMMA queryFilter)*;

queryFilter:
	/* missing "Target" segment*/ filterExpression /*missing annotations and filter restatement*/;
alias: IDENTIFIER;
entity: (schema DOT)? entity_name;
entity_name: IDENTIFIER;
schema: IDENTIFIER;

expressionContainer:
	expression (AS alias)? (WITH NATIVEREFERENCENAME)?;

// transform: TRANSFORM VIA STRING_LITERAL AS alias WITH --todo;
orderby:
	ORDERBY orderbySection (COMMA orderbySection)*;
groupby:
	ORDERBY expression direction (COMMA expression direction)*;
skip: SKIP_ INTEGER;
top: TOP INTEGER;


orderbySection: expression direction;
direction: ASCENDING | DESCENDING;
algorithm: IDENTIFIER;

select: SELECT expression (COMMA expression)*;

expression: filterExpression | nonFilterExpression;

nonPropertyExpression:
	aggregationExpr
	| arithmenticExpr
	| anyValueExpr
	| literalExpr
	| nullEpr
	| intExpr
	| decimalExpr
	| stringExpr
	| datetimeExpr
	| sourceRefExpr
	| datetimeSecExpr
	| dateExpr;

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
	| dateExpr
	| sourceRefExpr
	| propertyExpression;

filterExpression:
	| andExpr
	| orExpr
	| notExpr
	| betweenExpr
	| boolExp
	| inExpr
	| comparisonExpr
	| containsExpr;

sourceRefExpr: IDENTIFIER;
aggregationExpr: IDENTIFIER LPAREN expression RPAREN;
anyValueExpr: ANYVALUE WITH DEFAULTVALUEOVERRIDESANCESTORS;
andExpr: LPAREN left AND right RPAREN;
betweenExpr: nonFilterExpression BETWEEN ubound AND lbound;
nullEpr: NULL;
intExpr: INTEGER;
decimalExpr: DECIMAL;
datetimeExpr: DATETIME;
dateExpr: DATE;
datetimeSecExpr: DATETIMESECOND;
containsExpr: left CONTAINS right;
stringExpr: STRING_LITERAL;
boolExp: TRUE | FALSE;
orExpr: LPAREN left OR right RPAREN;
comparisonExpr: left operator right;
propertyExpression: nonPropertyExpression DOT IDENTIFIER;
notExpr: NOT LPAREN expression RPAREN;
literalExpr: STRING_LITERAL;
inExpr:
	(nonFilterExpression | (LPAREN nonFilterExpression (COMMA nonFilterExpression)* RPAREN )) 
	IN (tableName | inExprValues);
inExprValues: LPAREN expressionOrExpressionList (COMMA expressionOrExpressionList)* RPAREN (USING equalityKind)?;
expressionOrExpressionList
	: expression | LPAREN expression (COMMA expression)* RPAREN;
arithmenticExpr:
	LPAREN left BINARY_ARITHMETIC_OPERATOR right RPAREN;

orderByClause: ORDERBY expression;

tableName: IDENTIFIER;

equalityKind: IDENTIFIER;
left: nonFilterExpression;
right: nonFilterExpression;
ubound: nonFilterExpression;
lbound: nonFilterExpression;
operator: GT | LT | EQ | GTE | LTE;
