grammar VooDo;

// ---- Parser ----

expr:
	// Precedence 0
	NULL																	# NullLitExpr
	| value=BOOL															# BoolLitExpr
	| value=BIN_INT															# BinIntLitExpr
	| value=OCT_INT															# OctIntLitExpr
	| value=DEC_INT															# DecIntLitExpr
	| value=HEX_INT															# HexIntLitExpr
	| value=REAL															# RealLitExpr
	| value=STRING															# StringLitExpr
	| name=NAME																# NameExpr
	| '$' name=NAME															# ControllerNameExpr
	| '(' srcExpr=expr ')'													# GroupExpr
	| srcExpr=expr '.' memberName=NAME										# MemOpExpr
	| srcExpr=expr '(' ( argsExpr+=expr ( ',' argsExpr+=expr)*)? ')'		# CallOpExpr
	| srcExpr=expr '?' '(' ( argsExpr+=expr ( ',' argsExpr+=expr)*)? ')'	# NullableCallOpExpr
	| srcExpr=expr '[' (argsExpr+=expr ( ',' argsExpr+=expr)*)? ']'			# IndexExpr
	| srcExpr=expr '<' (argsExpr+=expr ( ',' argsExpr+=expr)*)? '>'			# SpecializationExpr
	| srcExpr=expr '?' '.' memberName=NAME									# NullableMemOpExpr
	| srcExpr=expr '?' '[' (argsExpr+=expr ( ',' argsExpr+=expr)*)? ']'		# NullableIndexExpr
	// Precedence 1
	| '+' srcExpr=expr														# PosOpExpr
	| '-' srcExpr=expr														# NegOpExpr
	| '!' srcExpr=expr														# LogNotExpr
	| '~' srcExpr=expr														# BwNegExpr
	// Precedence 2
	| lExpr=expr '*' rExpr=expr												# MulOpExpr
	| lExpr=expr '/' rExpr=expr												# DivOpExpr
	| lExpr=expr '%' rExpr=expr												# ModOpExpr
	// Precedence 3
	| lExpr=expr '+' rExpr=expr												# SumOpExpr
	| lExpr=expr '-' rExpr=expr												# SubOpExpr
	// Precedence 4
	| lExpr=expr '<<' rExpr=expr											# BwLstOpExpr
	| lExpr=expr '>>' rExpr=expr											# BwRstOpExpr
	// Precedence 5
	| lExpr=expr '<' rExpr=expr												# LtOpExpr
	| lExpr=expr '>' rExpr=expr												# GtOpExpr
	| lExpr=expr '<=' rExpr=expr											# LeOpExpr
	| lExpr=expr '>=' rExpr=expr											# GeOpExpr
	| srcExpr=expr IS typeExpr=expr											# IsExpr
	| srcExpr=expr AS typeExpr=expr											# CastExpr
	// Precedence 6
	| lExpr=expr '==' rExpr=expr											# EqOpExpr
	| lExpr=expr '!=' rExpr=expr											# NeqOpExpr
	// Precedence 7
	| lExpr=expr '&' rExpr=expr												# BwAndOpExpr
	// Precedence 8
	| lExpr=expr '^' rExpr=expr												# BwXorOpExpr
	// Precedence 9
	| lExpr=expr '|' rExpr=expr												# BwOrOpExpr
	// Precedence 10
	| lExpr=expr '&&' rExpr=expr											# LogAndOpExpr
	// Precedence 11
	| lExpr=expr '||' rExpr=expr											# LogOrOpExpr
	// Precedence 12
	| <assoc=right> srcExpr=expr '??' elseExpr=expr							# NullCoalOpExpr
	// Precedence 13
	| <assoc=right> condExpr=expr '?' thenExpr=expr ':' elseExpr=expr		# IfElseOpExpr
;

stat:
	'{' stats+=stat* '}'													# SequenceStat
	| IF '(' condExpr=expr ')' thenStat=stat ( ELSE elseStat=stat)?			# IfElseStat
	| WHILE '(' condExpr=expr ')' doStat=stat								# WhileStat
	| FOREACH '(' tgtExpr=expr IN srcExpr=expr ')' doStat=stat				# ForeachStat
	| tgtExpr=expr '=' srcExpr=expr ';'										# AssignmentStat
;

// ---- Lexer -----

// Keywords
IF: 'if';
ELSE: 'else';
WHILE: 'while';
FOREACH: 'foreach';
IN: 'in';
AS: 'as';
IS: 'is';

// Literals
NULL: 'null';
BOOL: 'true' | 'false';
BIN_INT: '0' [bB] [0-1]+;
OCT_INT: '0' [0-7]+;
DEC_INT: [0-9]+;
HEX_INT: '0' [xX] HexDigit+;
REAL: ([0-9]+ RealExponent) | ([0-9]* '.' [0-9]+ RealExponent?);
STRING: '"' (~["\\\r\n] | StringEscapeSequence)* '"';

// Whitespace and comments
WHITESPACE: [ \t\r\n\u000C]+ -> skip;
MULTILINE_COMMENT: '/*' .*? '*/' -> skip;
SINGLELINE_COMMENT: '//' ~[\r\n]* -> skip;

// Names
NAME: [@]? [a-zA-Z_] [0-9a-zA-Z_]*;

// Fragment rules

fragment StringEscapeSequence: ( '\\' [btnfr"'\\]) | ('\\' ([0-3]? [0-7])? [0-7]) | ('\\' 'u'+ HexDigit HexDigit HexDigit HexDigit);

fragment RealExponent: [eE] [+-]? [0-9]+;

fragment HexDigit: [0-9a-fA-F];
