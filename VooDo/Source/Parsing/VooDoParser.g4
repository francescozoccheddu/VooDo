parser grammar VooDoParser
;

options
{
	tokenVocab = VooDoLexer;
}

// Until EOF

script_Greedy
	: script EOF
;

usingDirective_Greedy
	: usingDirective EOF
;

statement_Greedy
	: statement EOF
;

expression_Greedy
	: expression EOF
;

identifier_Greedy
	: identifier EOF
;

identifierOrDiscard_Greedy
	: identifierOrDiscard EOF
;

namespace_Greedy
	: namespace EOF
;

simpleType_Greedy
	: simpleType EOF
;

complexType_Greedy
	: complexType EOF
;

complexTypeOrVar_Greedy
	: complexTypeOrVar EOF
;

qualifiedType_Greedy
	: qualifiedType EOF
;

tupleType_Greedy
	: tupleType EOF
;

complexTypeOrExpression_Greedy
	: complexTypeOrExpression EOF
;

// Script

script
	: mExpr = expression # inlineScript | mUsings += usingDirective* mBody += statement+ # fullScript
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
	| (declarationStatement | globalStatement)												# otherStatement
;

// Expressions

assignableExpression
	: nameOrMemberAccessExpression | tupleDeclarationExpression
;

elementAccessExpression
	: mSource = expression OPEN_BRACKET mArgs += expression (COMMA mArgs += expression)* CLOSE_BRACKET
;

argument
	: (mParam = identifier COLON)? mArgument = unnamedArgument
;

unnamedArgument
	: valueArgument | assignableArgument | outDeclarationArgument
;

valueArgument
	: mValue = expression
;

assignableArgument
	: mKind = (IN | REF | OUT)? mValue = expression
;

outDeclarationArgument
	: OUT mType = complexType mName = identifierOrDiscard	# outDeclarationArgumentWithType
	| OUT DISCARD											# outDeclarationArgumentWithDiscard
;

method
	: mSource = nameOrMemberAccessExpression (LT mTypeArgs += complexType (COMMA mTypeArgs += complexType)* GT)?
;

complexTypeOrExpression
	: complexType | expression
;

memberAccessExpression
	: mSource = complexType DOT mMember = identifier
;

nameExpression
	: mControllerOf = CONTROLLEROF? mName = identifier
;

nameOrMemberAccessExpression
	: nameExpression | memberAccessExpression
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
	| NEW mType = complexType? OPEN_PARENS (mArgs += argument (COMMA mArgs += argument)*)? CLOSE_PARENS									# objectCreationExpression
	| (assignableExpression | parenthesizedExpression)																					# otherExpression
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
	| mSource = expression OPEN_PARENS (mArgs += argument (COMMA mArgs += argument)*)? CLOSE_PARENS										# simpleInvocationExpression
	| mSource = method OPEN_PARENS (mArgs += argument (COMMA mArgs += argument)*)? CLOSE_PARENS											# methodInvocationExpression
	| OPEN_PARENS mType = complexType CLOSE_PARENS mExpr = expression																	# castExpression
	| mCond = expression QUEST mTrue = expression COLON mFalse = expression																# conditionalExpression
	| GLOB mController = expression (INIT mInitializer = expression)?																	# globalExpression
	| mSource = expression IS mType = complexType (mName = identifierOrDiscard)?														# isExpression
	| DEFAULT (OPEN_PARENS mType = complexType CLOSE_PARENS)																			# defaultExpression
	| OPEN_PARENS mElements += tupleExpressionElement (COMMA mElements += tupleExpressionElement)+ CLOSE_PARENS							# tupleExpression
	| mOp = (PLUS | MINUS | NOT | BNOT) mExpr = expression																				# unaryExpression
	| NULL																																					# nullLiteralExpression
	| FALSE																																					# falseLiteralExpression
	| TRUE																																					# trueLiteralExpression
	| mLiteral = (INTEGER_LITERAL | CHAR_LITERAL | REAL_LITERAL | BIN_INTEGER_LITERAL | HEX_INTEGER_LITERAL | STRING_LITERAL | VERBATIUM_STRING_LITERAL)	# otherLiteralExpression
;

// Directives

usingDirective
	: USING (mAlias = identifier ASSIGN)? mName = namespace SEMICOLON	# usingNamespaceDirective
	| USING STATIC mType = qualifiedTypeBase SEMICOLON					# usingStaticDirective
;

// Names

identifierOrDiscard
	: DISCARD | mIdentifier = identifier
;

complexTypeOrVar
	: mType = complexType | VAR
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
	: qualifiedTypeBase | tupleTypeBase
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
	: qualifiedType | tupleType
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
