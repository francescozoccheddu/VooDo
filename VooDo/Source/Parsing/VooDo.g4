grammar VooDo;

// ---- Parser ----

expr:
	NULL																# NullLitExpr
	| value=BOOL														# BoolLitExpr
	| value=BIN_INT														# BinIntLitExpr
	| value=OCT_INT														# OctIntLitExpr
	| value=DEC_INT														# DecIntLitExpr
	| value=HEX_INT														# HexIntLitExpr
	| value=REAL														# RealLitExpr
	| value=STRING														# StringLitExpr
	| path+=NAME ('@' path+=NAME)*										# NameExpr
	| '(' srcExpr=expr ')'												# GroupExpr
	| '(' typeExpr=expr ':' srcExpr=expr ')'							# CastExpr
	| '!' srcExpr=expr													# LogNotExpr
	| '+' srcExpr=expr													# PosOpExpr
	| '-' srcExpr=expr													# NegOpExpr
	| '~' srcExpr=expr													# BwNegExpr
	| srcExpr=expr '[' (argsExpr+=expr ( ',' argsExpr+=expr)*)? ']'		# IndexExpr
	| lExpr=expr '*' rExpr=expr											# MulOpExpr
	| lExpr=expr '/' rExpr=expr											# DivOpExpr
	| lExpr=expr '+' rExpr=expr											# SumOpExpr
	| lExpr=expr '-' rExpr=expr											# SubOpExpr
	| lExpr=expr '%' rExpr=expr											# ModOpExpr
	| lExpr=expr '&' rExpr=expr											# BwAndOpExpr
	| lExpr=expr '|' rExpr=expr											# BwOrOpExpr
	| lExpr=expr '^' rExpr=expr											# BwXorOpExpr
	| lExpr=expr '<<' rExpr=expr										# BwLstOpExpr
	| lExpr=expr '>>' rExpr=expr										# BwRstOpExpr
	| lExpr=expr '&&' rExpr=expr										# LogAndOpExpr
	| lExpr=expr '||' rExpr=expr										# LogOrOpExpr
	| lExpr=expr '<' rExpr=expr											# LtOpExpr
	| lExpr=expr '>' rExpr=expr											# GtOpExpr
	| lExpr=expr '<=' rExpr=expr										# LeOpExpr
	| lExpr=expr '>=' rExpr=expr										# GeOpExpr
	| lExpr=expr '==' rExpr=expr										# EqOpExpr
	| lExpr=expr '!=' rExpr=expr										# NeqOpExpr
	| srcExpr=expr '.' memberExpr=expr									# MemOpExpr
	| srcExpr=expr '?.' memberExpr=expr									# NullableMemOpExpr
	| srcExpr=expr '??' elseExpr=expr									# NullCoalOpExpr
	| condExpr=expr '?' thenExpr=expr ':' elseExpr=expr					# IfElseOpExpr
	| srcExpr=expr '(' ( argsExpr+=expr ( ',' argsExpr+=expr)*)? ')'	# CallOpExpr
;

stat:
	'{' stats+=stat* '}'											# SequenceStat
	| IF '(' condExpr=expr ')' thenStat=stat ( ELSE elseStat=stat)?	# IfElseStat
	| WHILE '(' condExpr=expr ')' doStat=stat						# WhileStat
	| FOREACH '(' tgtExpr=expr IN srcExpr=expr ')' doStat=stat		# ForeachStat
	| tgtExpr=expr '=' srcExpr=expr ';'								# AssignmentStat
	| tgtExpr=expr ':=' srcExpr=expr ';'							# LinkStat
;

// ---- Lexer -----

// Keywords
IF: 'if';
ELSE: 'else';
WHILE: 'while';
FOREACH: 'foreach';
IN: 'in';

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
NAME: [a-zA-Z_] [0-9a-zA-Z_]*;

// Fragment rules

fragment StringEscapeSequence: (
		'\\' [btnfr"'\\]
	)
	| ('\\' ([0-3]? [0-7])? [0-7])
	| ('\\' 'u'+ HexDigit HexDigit HexDigit HexDigit)
;

fragment RealExponent: [eE] [+-]? [0-9]+;

fragment HexDigit: [0-9a-fA-F];
