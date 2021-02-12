parser grammar VooDoParser
;

options
{
	tokenVocab = VooDoLexer;
}

// Script

script
	: mUsings += usingDirective* mBody = statement* # fullScript | mExpr = expression # inlineScript
;

// Statements

declarationStatement
	: mType = complexTypeOrVar mDeclarators += declarator (COMMA mDeclarators += declarator)* SEMICOLON
;

declarator
	: mName = identifier (ASSIGN mInitializer = expression)?
;

globalStatement
	: GLOBAL OPEN_BRACE mDeclarations += declarationStatement* CLOSE_BRACE
	| GLOBAL mDeclarations += declarationStatement
;

statement
	: mTarget = assignableExpression mOp =
	(
		ASSIGN
		| ASSIGN_ADD
		| ASSIGN_AND
		| ASSIGN_COAL
		| ASSIGN_DIV
		| ASSIGN_LSH
		| ASSIGN_MOD
		| ASSIGN_MUL
		| ASSIGN_OR
		| ASSIGN_RSH
		| ASSIGN_SUB
		| ASSIGN_XOR
	) mSource = expression SEMICOLON														# assignmentStatement
	| mExpr = expression SEMICOLON															# expressionStatement
	| OPEN_BRACE mStatements += statement* CLOSE_BRACE										# blockStatement
	| IF mCondition = parenthesizedExpression mThen = statement (ELSE mElse = statement)?	# ifStatement
	| RETURN mExpr = expression SEMICOLON													# returnStatement
	| (mVar1 = declarationStatement | mVar2 = globalStatement)								# otherStatement
;

// Expressions

assignableExpression
	: mVar1 = nameExpression | mVar2 = memberAccessExpression | mVar3 = tupleDeclarationExpression
;

elementAccessExpression
	: mSource = expression OPEN_BRACKET mArgs += expression (COMMA mArgs += expression)* CLOSE_BRACKET
;

argument
	: (mParam = identifier COLON)? unnamedArgument
;

unnamedArgument
	: mVar1 = valueArgument | mVar2 = assignableArgument | mVar3 = outDeclarationArgument
;

valueArgument
	: mValue = expression
;

assignableArgument
	: mKind = (IN | REF | OUT)? mValue = expression
;

outDeclarationArgument
	: OUT mType = complexType mName = identifierOrDiscard
;

method
	: mSource = nameOrMemberAccessExpression (LT mTypeArgs += complexType (COMMA mTypeArgs += complexType)* GT)?
;

complexTypeOrExpression
	: mVar1 = complexType | mVar2 = expression
;

memberAccessExpression
	: mSource = complexType DOT mMember = identifier
;

nameExpression
	: mControllerOf = CONTROLLEROF? mName = identifier
;

nameOrMemberAccessExpression
	: mVar1 = nameExpression | mVar2 = memberAccessExpression
;

parenthesizedExpression
	: OPEN_PARENS mExpr = expression CLOSE_PARENS
;

tupleExpressionElement
	: (mName = identifier COLON)? mExpr = expression
;

tupleDeclarationExpressionElement
	: mType = complexTypeOrVar mName = identifierOrDiscard
;

tupleDeclarationExpression
	: OPEN_PARENS mElements += tupleDeclarationExpressionElement (COMMA mElements += tupleDeclarationExpressionElement)+ CLOSE_PARENS
;

expression
	: NEW mType = complexType OPEN_BRACKET mSizes += expression (COMMA mSizes += expression)* CLOSE_BRACKET mRanks += rankSpecifier*	# arrayCreationExpression
	| mVar1 = assignableExpression																										# otherExpression
	| mExpr = expression AS mType = complexType																							# asExpression
	| mLeft = expression mOp = (MUL | DIV | MOD) mRight = expression																	# binaryExpression
	| mLeft = expression mOp = (PLUS | MINUS) mRight = expression																		# binaryExpression
	| mLeft = expression mOp = (LSH | RSH) mRight = expression																			# binaryExpression
	| mLeft = expression mOp = (LT | LE | GT | GE) mRight = expression																	# binaryExpression
	| mLeft = expression mOp = (EQ | NEQ) mRight = expression																			# binaryExpression
	| mLeft = expression mOp = AND mRight = expression																					# binaryExpression
	| mLeft = expression mOp = XOR mRight = expression																					# binaryExpression
	| mLeft = expression mOp = OR mRight = expression																					# binaryExpression
	| mLeft = expression mOp = LAND mRight = expression																					# binaryExpression
	| mLeft = expression mOp = LOR mRight = expression																					# binaryExpression
	| mLeft = expression mOp = COAL mRight = expression																					# binaryExpression
	| OPEN_PARENS mType = complexType CLOSE_PARENS mExpr = expression																	# castExpression
	| mCond = expression QUEST mTrue = expression COLON mFalse = expression																# conditionalExpression
	| mSource = expression OPEN_PARENS (mArgs += argument (COMMA mArgs += argument)*)? CLOSE_PARENS										# simpleInvocationExpression
	| mSource = method OPEN_PARENS (mArgs += argument (COMMA mArgs += argument)*)? CLOSE_PARENS											# methodInvocationExpression
	| GLOB mController = expression (INIT mInitializer = expression)																	# globalExpression
	| mSource = expression IS mType = complexType (mName = identifierOrDiscard)?														# isExpression
	| DEFAULT (OPEN_PARENS mType = complexType CLOSE_PARENS)																			# defaultExpression
	| OPEN_PARENS mElements += tupleExpressionElement (COMMA mElements += tupleExpressionElement)+ CLOSE_PARENS							# tupleExpression
	| mOp = (PLUS | MINUS | NOT | BNOT) mExpr = expression																				# unaryExpression
	| NULL																																# nullLiteralExpression
	| FALSE																																# falseLiteralExpression
	| TRUE																																# trueLiteralExpression
	| mLiteral = (INTEGER_LITERAL | CHAR_LITERAL | REAL_LITERAL | BIN_INTEGER_LITERAL | HEX_INTEGER_LITERAL)							# otherLiteralExpression
;

// Directives

usingDirective
	: USING (mAlias = identifier ASSIGN)? namespace SEMICOLON	# usingNamespaceDirective
	| USING STATIC qualifiedTypeBase SEMICOLON					# usingStaticDirective
;

// Names

identifierOrDiscard
	: mVar1 = DISCARD | mVar2 = identifier
;

complexTypeOrVar
	: mVar1 = complexType | mVar2 = VAR
;

qualifiedTypeBase
	: (mAlias = identifier DCOLON)? mPath += simpleType (DOT mPath += simpleType)*
;

tupleTypeBase
	: OPEN_PARENS mElements += tupleTypeElement (COMMA mElements += tupleTypeElement)+ CLOSE_PARENS
;

tupleTypeElement
	: mType = complexType mName = identifier?
;

complexTypeBase
	: mVar1 = qualifiedTypeBase | mVar2 = tupleTypeBase
;

rankSpecifier
	: OPEN_BRACE mCommas += COMMA CLOSE_BRACE
;

tupleType
	: mBase = tupleTypeBase (mNullable = QUEST)? mRanks += rankSpecifier*
;

qualifiedType
	: mBase = qualifiedTypeBase (mNullable = QUEST)? mRanks += rankSpecifier*
;

complexType
	: mVar1 = qualifiedType | mVar2 = tupleType
;

simpleType
	: mName = identifier (LT mTypeArgs += complexType (COMMA mTypeArgs += complexType)* GT)?
;

namespace
	: (mAlias = identifier DCOLON)? mPath += identifier (DOT mPath += identifier)*
;

identifier
	: IDENTIFIER | STATIC | USING | DISCARD
;
