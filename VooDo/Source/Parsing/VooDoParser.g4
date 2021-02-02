parser grammar VooDoParser
;

options
{
	tokenVocab = VooDoLexer;
}

// Entry point

script
	: mExterns = externAliasDirective* mUsings = usingDirective* mGlobals = globalsBlock? mStatements += statement* EOF
; 

inlineScript
	: externAliasDirective* mUsings = usingDirective* mGlobals = globalsBlock? mStatements += statement* mReturn = expression EOF
;

globalsBlock
	: GLOBALS OPEN_BRACE statements = implicitGlobalDeclarationStatement* CLOSE_BRACE
;

// Basic concepts

genericName
	: mName = name mTypeArguments = typeArgumentList?
;

aliasGenericName
	: (mAlias = name DCOLON)? mName = genericName
;

qualifiedAliasGenericName
	: mPrimary = aliasGenericName (DOT mParts += genericName)*
;

// Types

type
	: mType = nonArrayType mRanks += arrayRankSpecifier* mNullable = INTERR?
;

nonArrayType
	: mType1 = predefinedStructType | mType2 = classType | mType3 = tupleType
;

tupleType
	: OPEN_PARENS mElement += tupleTypeElement (COMMA mElement += tupleTypeElement)+ CLOSE_PARENS
;

tupleTypeElement
	: mType = type mName = name?
;

predefinedStructType
	: mType = (SBYTE | BYTE | SHORT | USHORT | INT | UINT | LONG | ULONG | CHAR | DECIMAL | FLOAT | DOUBLE | BOOL)
;

predefinedClassType
	: mType = (OBJECT | STRING)
;

predefinedType
	: mType1 = predefinedClassType | mType2 = predefinedStructType
;

classType
	: mType1 = qualifiedAliasGenericName | mType2 = predefinedClassType
;

typeArgumentList
	: LT mTypes += type (COMMA mTypes += type)* GT
;

// Expressions

argumentList
	: mArguments += argument (COMMA mArguments += argument)*
;

argument
	: (mParam = name COLON)? mKind = (REF | IN)? mExpr = expression
	| (mParam = name COLON)? mKind = OUT mType = typeOrVar? mExpr = expression
;

bracketExpressionSuffix
	: mNullable = INTERR? mExpr = nonnullBracketExpressionSuffix
;

nonnullBracketExpressionSuffix
	: OPEN_BRACKET mArguments += indexerArgument ( COMMA mArguments += indexerArgument)* CLOSE_BRACKET
;

indexerArgument
	: (mParam = name COLON)? mExpr = expression
;

expression
	: mLiteral = literal																	# literalExpression
	| CONTROLLEROF mName = name																# controllerofExpression
	| mName = name																			# nameExpression
	| OPEN_PARENS mExpr = expression CLOSE_PARENS											# parensExpression
	| OPEN_PARENS mElements += expression ( COMMA mElements += expression)+ CLOSE_PARENS	# tupleExpression
	| DEFAULT (OPEN_PARENS mType = type CLOSE_PARENS)?										# defaultExpression
	| NAMEOF OPEN_PARENS (mParts += name DOT)* mParts += name CLOSE_PARENS					# nameofExpression
	| mSrc = expression mNullable = INTERR? DOT mMemb = genericName							# memberAccessExpression
	| mSrc = typeMemberAccessExpressionSource DOT mMemb = genericName						# typeMemberAccessExpression
	| mSrc = expression mIndexers += bracketExpressionSuffix+								# bracketExpression
	| PLUS mSrc = expression																# unaryPlusExpression
	| MINUS mSrc = expression																# unaryMinusExpression
	| NOT mSrc = expression																	# notExpression
	| BCOMPL mSrc = expression																# bcomplExpression
	| OPEN_PARENS mType = type CLOSE_PARENS mSrc = expression								# castExpression
	| mLeft = expression mOp = (MUL | DIV | MOD) mRight = expression						# multiplicativeExpressionGroup
	| mLeft = expression mOp = (PLUS | MINUS) mRight = expression							# additiveExpressionGroup
	| mLeft = expression mOp = (LSH | RSH) mRight = expression								# shiftExpressionGroup
	| mLeft = expression mOp = (LT | GT | LE | GE) mRight = expression						# comparisonExpressionGroup
	| mLeft = expression mOp = (EQ | NEQ) mRight = expression								# equalityExpressionGroup
	| mLeft = expression AND mRight = expression											# andExpression
	| mLeft = expression XOR mRight = expression											# xorExpression
	| mLeft = expression OR mRight = expression												# orExpression
	| mLeft = expression LAND mRight = expression											# landExpression
	| mLeft = expression LOR mRight = expression											# lorExpression
	| mSrc = expression NULLC mElse = expressionOrThrow										# nullcExpression
	| mCond = expression INTERR mThen = expressionOrThrow COLON mElse = expressionOrThrow	# conditionalExpression
;

typeMemberAccessExpressionSource
	: mType1 = predefinedType | mType2 = aliasGenericName
;

expressionOrThrow
	: mExpr1 = expression | mExpr2 = throwExpression
;

throwExpression
	: THROW mSrc = expression
;

// Arrays
arrayRankSpecifier
	: OPEN_BRACKET mCommas += COMMA* CLOSE_BRACKET
;

arrayInitializer
	: OPEN_BRACE (mElements += variableInitializer (COMMA mElements += variableInitializer)* COMMA?)? CLOSE_BRACE
;

// Statements

block
	: OPEN_BRACE mStatements += statement* CLOSE_BRACE
;

statement
	: mTgt = expression mOp =
	(
		ASSIGN
		| ASSIGN_ADD
		| ASSIGN_AND
		| ASSIGN_DIV
		| ASSIGN_LSH
		| ASSIGN_MOD
		| ASSIGN_MUL
		| ASSIGN_NULLC
		| ASSIGN_OR
		| ASSIGN_RSH
		| ASSIGN_SUB
		| ASSIGN_XOR
	) mSrc = expression																						# assignmentStatement
	| mExpr = expression SEMICOLON																			# expressionStatement
	| mSrc = variableDeclaration SEMICOLON																	# variableDeclarationStatement
	| GLOBAL mSrc = variableDeclaration SEMICOLON															# globalDeclarationStatement
	| CONST mSrc = constantDeclaration SEMICOLON															# constantDeclarationStatement
	| IF OPEN_PARENS expression CLOSE_PARENS statement (ELSE statement)?									# ifStatement
	| RETURN expression SEMICOLON																			# returnStatement
	| THROW expression? SEMICOLON																			# throwStatement
	| TRY mBlock = block (mCatches = catchClauseList mFinally = finallyClause? | mFinally = finallyClause)	# tryStatement
;

catchClauseList
	: mClauses += specificCatchClause (mClauses += specificCatchClause)* mLastClause = generalCatchClause?	# multiCatch
	| mClause += generalCatchClause																			# allCatch
;

specificCatchClause
	: CATCH OPEN_PARENS mType = classType mName = name? CLOSE_PARENS mBlock = block
;

generalCatchClause
	: CATCH mBlock = block
;

finallyClause
	: FINALLY mBlock = block
;

implicitGlobalDeclarationStatement
	: mSrc = variableDeclaration SEMICOLON
;

typeOrVar
	: mType = type # explicityVariableType | VAR # inferredVariableType
;

constantDeclarator
	: mTgt = name ASSIGN mSrc = expression
;

variableDeclarator
	: mTgt = name (ASSIGN mSrc = variableInitializer)?
;

variableInitializer
	: mSrc = expression # expressionVariableInitializer | mSrc = arrayInitializer # arrayVariableInitializer
;

constantDeclaration
	: mType = typeOrVar mDeclarators += constantDeclarator (COMMA mDeclarators += constantDeclarator)*
;

variableDeclaration
	: mType = typeOrVar mDeclarators += variableDeclarator ( COMMA mDeclarators += variableDeclarator)*
;

// Namespaces

externAliasDirective
	: EXTERN ALIAS mName = name SEMICOLON
;

usingDirective
	: USING mName = name SEMICOLON											# UsingAlias
	| USING mStatic = STATIC? mName = qualifiedAliasGenericName SEMICOLON	# Using
;

literal
	: mSrc = (TRUE | FALSE)							# boolLiteral
	| mSrc = (REGULAR_STRING | VERBATIUM_STRING)	# stringLiteral
	| mSrc = DEC_INTEGER_LITERAL					# decIntegerLiteral
	| mSrc = HEX_INTEGER_LITERAL					# hexIntegerLiteral
	| mSrc = BIN_INTEGER_LITERAL					# binIntegerLiteral
	| mSrc = REAL_LITERAL							# realLiteral
	| mSrc = CHAR_LITERAL							# charLiteral
	| NULL											# nullLiteral
;

name
	: IDENTIFIER | ALIAS | EXTERN | GLOBAL | GLOBALS | STATIC
;