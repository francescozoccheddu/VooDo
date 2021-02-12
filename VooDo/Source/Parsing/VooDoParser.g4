parser grammar VooDoParser
;

options
{
	tokenVocab = VooDoLexer;
}

// Script

script
	: mVar1 = fullScript | mVar2 = inlineScript
;

fullScript
	: mUsings += usingDirective* mBody = blockStatement
;

inlineScript
	: mExpr = expression
;

// Statements

assignmentStatement
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
	) mSource = expression SEMICOLON
;

blockStatement
	: OPEN_BRACE mStatements += statement* CLOSE_BRACE
;

declarationStatement
	: mType = complexTypeOrExpression mDeclarators += declarator (COMMA mDeclarators += declarator)* SEMICOLON
;

declarator
	: mName = identifier (ASSIGN mInitializer = expression)?
;

expressionStatement
	: mExpr = expression SEMICOLON
;

globalStatement
	: GLOBAL OPEN_BRACE mDeclarations += declarationStatement* CLOSE_BRACE
	| GLOBAL mDeclarations += declarationStatement
;

ifStatement
	: IF mCondition = parenthesizedExpression mThen = statement (ELSE mElse = statement)?
;

returnStatement
	: RETURN mExpr = expression SEMICOLON
;

statement
	: mVar1 = assignmentStatement
	| mVar2 = blockStatement
	| mVar3 = expressionStatement
	| mVar4 = declarationStatement
	| mVar5 = globalStatement
	| mVar6 = ifStatement
	| mVar7 = returnStatement
;

// Expressions

arrayCreationExpression
	: NEW mType = complexType OPEN_BRACKET mSizes += expression (COMMA mSizes += expression)* CLOSE_BRACKET mRanks += rankSpecifier*
;

asExpression
	: expression AS complexType
;

assignableExpression
	: mVar1 = nameExpression | mVar2 = memberAccessExpression
;

binaryExpression
	: mLeft = expression mOp = (MUL | DIV | MOD) mRight = expression
	| mLeft = expression mOp = (PLUS | MINUS) mRight = expression
	| mLeft = expression mOp = (LSH | RSH) mRight = expression
	| mLeft = expression mOp = (LT | LE | GT | GE) mRight = expression
	| mLeft = expression mOp = (EQ | NEQ) mRight = expression
	| mLeft = expression mOp = AND mRight = expression
	| mLeft = expression mOp = XOR mRight = expression
	| mLeft = expression mOp = OR mRight = expression
	| mLeft = expression mOp = LAND mRight = expression
	| mLeft = expression mOp = LOR mRight = expression
	| mLeft = expression mOp = COAL mRight = expression
;

castExpression
	: OPEN_PARENS mType = complexType CLOSE_PARENS mExpr = expression
;

conditionalExpression
	: mCond = expression QUEST mTrue = expression COLON mFalse = expression
;

defaultExpression
	: DEFAULT (OPEN_PARENS mType = complexType CLOSE_PARENS)
;

elementAccessExpression
	: mSource = expression OPEN_BRACKET mArgs += expression (COMMA mArgs += expression)* CLOSE_BRACKET
;

globalExpression
	: GLOB mController = expression (INIT mInitializer = expression)
;

argument
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

callable
	: mVar1 = simpleCallable | mVar2 = method
;

simpleCallable
	: mSource = expression
;

method
	: mSource = nameOrMemberAccessExpression (LT mTypeArgs += complexType (COMMA mTypeArgs += complexType)* GT)?
;

invocationExpression
	: mSource = callable OPEN_PARENS (mArgs += argument (COMMA mArgs += argument)*)? CLOSE_PARENS
;

isExpression
	: mSource = expression IS mType = complexType (mName = identifierOrDiscard)?
;

literalExpression
	: NULL																										# nullLiteralExpression
	| FALSE																										# falseLiteralExpression
	| TRUE																										# trueLiteralExpression
	| mLiteral = (INTEGER_LITERAL | CHAR_LITERAL | REAL_LITERAL | BIN_INTEGER_LITERAL | HEX_INTEGER_LITERAL)	# otherLiteralExpression
;

complexTypeOrExpression
	: mVar1 = complexType | mVar2 = expression
;

memberAccessExpression
	: mSource = complexTypeOrExpression DOT mMember = identifier
;

nameExpression
	: mControllerOf = CONTROLLEROF? mName = identifier
;

nameOrMemberAccessExpression
	: mVar1 = nameExpression | mVar2 = memberAccessExpression
;

objectCreationExpression
	: NEW mType = complexTypeBase? OPEN_PARENS (mArgs += argument (COMMA mArgs += argument)*)? CLOSE_PARENS
;

tupleExpression
	: OPEN_PARENS mElements += expression (COMMA mElements += expression)+ CLOSE_PARENS
;

unaryExpression
	: mOp = (PLUS | MINUS | NOT | BNOT) mExpr = expression
;

parenthesizedExpression
	: OPEN_PARENS mExpr = expression CLOSE_PARENS
;

expression
	: mVar1 = arrayCreationExpression
	| mVar2 = asExpression
	| mVar3 = assignableExpression
	| mVar4 = binaryExpression
	| mVar5 = castExpression
	| mVar6 = conditionalExpression
	| mVar7 = defaultExpression
	| mVar8 = globalExpression
	| mVar9 = invocationExpression
	| mVar10 = isExpression
	| mVar11 = literalExpression
	| mVar12 = objectCreationExpression
	| mVar13 = tupleExpression
	| mVar14 = unaryExpression
	| mVar15 = parenthesizedExpression
;

// Directives

usingDirective
	: mVar1 = usingNamespaceDirective | mVar2 = usingStaticDirective
;

usingNamespaceDirective
	: USING (mAlias = identifier ASSIGN)? namespace SEMICOLON
;

usingStaticDirective
	: USING STATIC qualifiedTypeBase SEMICOLON
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
