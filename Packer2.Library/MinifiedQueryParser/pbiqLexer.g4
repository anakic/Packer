lexer grammar pbiqLexer;

FROM: [Ff] [Rr] [Oo] [Mm];
AS: [Aa] [Ss];
IN: [Ii] [Nn];
WITH: [Ww] [Ii] [Tt] [Hh];
NATIVEREFERENCENAME: [Nn] [Aa] [Tt] [Ii] [Vv] [Ee] [Rr] [Ee] [Ff] [Ee] [Rr] [Ee] [Nn] [Cc] [Ee] [Nn] [Aa] [Mm] [Ee];
DOT: [.];
AND: [Aa] [Nn] [Dd];
OR: [Oo] [Rr];
NOT: [Nn] [Oo] [Tt];
WHERE: [Ww] [Hh] [Ee] [Rr] [Ee];
ORDERBY: [Oo] [Rr] [Dd] [Ee] [Rr] [Bb] [Yy];
GROUPBY: [Gg] [Rr] [Oo] [Uu] [Pp] [Bb] [Yy];
ASCENDING:[Aa] [Ss] [Cc] [Ee] [Nn] [Dd] [Ii] [Nn] [Gg];
DESCENDING:[Dd] [Ee] [Ss] [Cc] [Ee] [Nn] [Dd] [Ii] [Nn] [Gg];
SELECT: [Ss] [Ee] [Ll] [Ee] [Cc] [Tt];
SKIP_: [Ss] [Kk] [Ii] [Pp];
TOP: [Tt] [Oo] [Pp];
SCOPE: [Ss] [Cc] [Oo] [Pp] [Ee];
SCOPEDEVAL: [Ss] [Cc] [Oo] [Pp] [Ee] [Dd] [Ee] [Vv] [Aa] [Ll];
DATESPAN: [Dd] [Aa] [Tt] [Ee] [Ss] [Pp] [Aa] [Nn];
USING: [Uu] [Ss] [Ii] [Nn] [Gg];
ANYVALUE: [Aa] [Nn] [Yy] [Vv] [Aa] [Ll] [Uu] [Ee];
VARIATION: [Vv] [Aa] [Rr] [Ii] [Aa] [Tt] [Ii] [Oo] [Nn];
DEFAULTVALUEOVERRIDESANCESTORS: [Dd] [Ee] [Ff] [Aa] [Uu] [Ll] [Tt] [Vv] [Aa] [Ll] [Uu] [Ee] [Oo] [Vv] [Ee] [Rr] [Rr] [Ii] [Dd] [Ee] [Ss] [Aa] [Nn] [Cc] [Ee] [Ss] [Tt] [Oo] [Rr] [Ss];
TRANSFORM: [Tt] [Rr] [Aa] [Nn] [Ss] [Ff] [Oo] [Rr] [Mm];
VIA: [Vv] [Ii] [Aa];
NULL: [Nn] [Uu] [Ll] [Ll];
TRUE: [Tt] [Rr] [Uu] [Ee];
FALSE: [Ff] [Aa] [Ll] [Ss] [Ee];
BETWEEN: [Bb] [Ee] [Tt] [Ww] [Ee] [Ee] [Nn];
CONTAINS: [Cc] [Oo] [Nn] [Tt] [Aa] [Ii] [Nn] [Ss];
HIERARCHY: [Hh] [Ii] [Ee] [Rr] [Aa] [Rr] [Cc] [Hh] [Yy];
LEVEL: [Ll] [Ee] [Vv] [Ee] [Ll];
AS_: [Aa] [Ss];
INPUTPARAMETERS: [Ii] [Nn] [Pp] [Uu] [Tt] [Pp] [Aa] [Rr] [Aa] [Mm] [Ee] [Tt] [Ee] [Rr] [Ss];
INPUTTABLE: [Ii] [Nn] [Pp] [Uu] [Tt] [Tt] [Aa] [Bb] [Ll] [Ee];
OUTPUTTABLE: [Oo] [Uu] [Tt] [Pp] [Uu] [Tt] [Tt] [Aa] [Bb] [Ll] [Ee];
TRANSFORMOUTPUTROLE: [Tt] [Rr] [Aa] [Nn] [Ss] [Ff] [Oo] [Rr] [Mm] [Oo] [Uu] [Tt] [Pp] [Uu] [Tt] [Rr] [Oo] [Ll] [Ee];
ROLE: [Rr] [Oo] [Ll] [Ee];
INTEGER: '-'? [0-9]+;
INTEGER_LITERAL: INTEGER [Ll];
DOUBLE_LITERAL: INTEGER ('.' [0-9]*)? [Dd];
DECIMAL_LITERAL: INTEGER ('.' [0-9]*)? [Mm];
DATE: [0-9] [0-9] [0-9] [0-9] '-' [0-9] [0-9] '-' [0-9] [0-9];
DATETIME: DATE 'T' [0-9] [0-9] ':' [0-9] [0-9] ':' [0-9] [0-9] ('.' [0-9]+)?;
BASE64BYTES_LITERAL:[Bb] [Aa] [Ss] [Ee] '6' '4' '\'' STRING_LITERAL '\'';
DATEIME_LITERAL: [Dd] [Aa] [Tt] [Ee] [Tt] [Ii] [Mm] [Ee] '\'' DATETIME '\'';
ROLEREF: [Rr] [Oo] [Ll] [Ee] [Rr] [Ee] [Ff];
LPAREN: '(';
RPAREN: ')';
LCURLY: '{';
RCURLY: '}';
COMMA: ',';
GT: '>';
GTE: '>=';
LT: '<';
LTE: '<=';
EQ: '=';
STRING_LITERAL: '\'' (~[']|'\'' '\'')* '\'';
IDENTIFIER : ([A-Za-z0-9_]+);
QUOTED_IDENTIFIER: ('[' ~[\]]+? ']');
PLUS: '+';
MINUS: '-';
MULT: '*';
DIV: '/';

WS: [ \r\n\t] -> channel(HIDDEN);

