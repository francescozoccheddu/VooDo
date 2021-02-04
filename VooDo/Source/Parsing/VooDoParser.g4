parser grammar VooDoParser
;

options
{
	tokenVocab = VooDoLexer;
}

// Entry point

script
	: mUsings += usingDirective* mBody = scriptBody EOF
;

inlineScript
	: mSrc = expression
;

scriptBody
	: mGlobals = globalsBlock? mStatements += statement*
;

globalsBlock
	: mGlobals = GLOBALS mBraceO = OPEN_BRACE mStatements += implicitGlobalDeclarationStatement* mBraceC = CLOSE_BRACE
;

// Basic concepts

genericName
	: mName = name mTypeArguments = typeArgumentList?
;

aliasGenericName
	: (mAlias = name mDCol = DCOLON)? mName = genericName
;

qualifiedAliasGenericName
	: mLeft = qualifiedAliasGenericName DOT mRight = genericName # qualifiedName | mSrc = aliasGenericName # qualifiedGenericNamePass
;

// Types

type
	: mType = nullableType mRanks += arrayRankSpecifier*
;

typeWithSize
	: mType = nullableType mSize = arraySizeSpecifier mRanks += arrayRankSpecifier*
;

nullableType
	: mType = nonArrayType mQuest = INTERR?
;

nonArrayType
	: mType1 = predefinedStructType | mType2 = classType | mType3 = tupleType
;

tupleType
	: mParenO = OPEN_PARENS mElements += tupleTypeElement (mCommas += COMMA mElements += tupleTypeElement)+ mParenC = CLOSE_PARENS
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
	: mLt = LT mTypes += type (mCommas += COMMA mTypes += type)* mGt = GT
;

// Expressions

argumentList
	: mParenO = OPEN_PARENS (mArguments += argument (mCommas += COMMA mArguments += argument)*)? mParenC = CLOSE_PARENS
;

argument
	: mParam = nameColon? mKind = (REF | IN)? mExpr = expression | mParam = nameColon? mKind = OUT (mDeclExpr = declarationExpression | mExpr = expression)
;

declarationExpression
	: mType = typeOrVar mName = name
;

bracketedArgumentList
	: mBracketO = OPEN_BRACKET mArguments += indexerArgument ( mCommas += COMMA mArguments += indexerArgument)* mBracketC = CLOSE_BRACKET
;

indexerArgument
	: mParam = nameColon? mIn = IN? mExpr = expression
;

nameColon
	: mName = name mColon = COLON
;

expression
	: mSrc = (REGULAR_STRING | VERBATIUM_STRING | INTEGER_LITERAL | HEX_INTEGER_LITERAL | BIN_INTEGER_LITERAL | REAL_LITERAL | CHAR_LITERAL)	# literalExpression
	| mKeyword = (NULL | TRUE | FALSE | DEFAULT)																								# keywordLiteralExpression
	| mOp = CONTROLLEROF mName = name																											# controllerofExpression
	| mGlob = GLOB mContr = expression (mInit = INIT mInitializer = expression)?																# globExpression
	| mName = name																																# nameExpression
	| mNew = NEW mType = nonArrayType mArgumentList = argumentList																				# objectCreationExpression
	| mNew = NEW mArgumentList = argumentList																									# implicitObjectCreationExpression
	| mNew = NEW mType = typeWithSize mInitializer = arrayInitializer?																			# arrayCreationExpression
	| mSrc = expression mArgumentList = argumentList																							# invocationExpression
	| mParenO = OPEN_PARENS mExpr = expression mParenC = CLOSE_PARENS																			# parensExpression
	| mParenO = OPEN_PARENS mElements += expression (mCommas += COMMA mElements += expression)+ mParenC = CLOSE_PARENS							# tupleExpression
	| mDefault = DEFAULT (mParenO = OPEN_PARENS mType = type mParenC = CLOSE_PARENS)															# defaultExpression
	| mSrc = expression mNullable = INTERR? mDot = DOT mMemb = genericName																		# memberAccessExpression
	| mSrc = typeMemberAccessExpressionSource mDot = DOT mMemb = genericName																	# typeMemberAccessExpression
	| mSrc = expression mNullable = INTERR? mArgList = bracketedArgumentList																	# elementAccessExpression
	| mOp = PLUS mSrc = expression																												# unaryPlusExpression
	| mOp = MINUS mSrc = expression																												# unaryMinusExpression
	| mOp = NOT mSrc = expression																												# notExpression
	| mOp = BCOMPL mSrc = expression																											# bcomplExpression
	| mParenO = OPEN_PARENS mType = type mParenC = CLOSE_PARENS mSrc = expression																# castExpression
	| mSrc = expression mOp = AS mType = type																									# asExpression
	| mSrc = expression mOp = IS mType = type mName = name?																						# isExpression
	| mLeft = expression mOp = (MUL | DIV | MOD) mRight = expression																			# multiplicativeExpressionGroup
	| mLeft = expression mOp = (PLUS | MINUS) mRight = expression																				# additiveExpressionGroup
	| mLeft = expression mOp = (LSH | RSH) mRight = expression																					# shiftExpressionGroup
	| mLeft = expression mOp = (LT | GT | LE | GE) mRight = expression																			# comparisonExpressionGroup
	| mLeft = expression mOp = (EQ | NEQ) mRight = expression																					# equalityExpressionGroup
	| mLeft = expression mOp = AND mRight = expression																							# andExpression
	| mLeft = expression mOp = XOR mRight = expression																							# xorExpression
	| mLeft = expression mOp = OR mRight = expression																							# orExpression
	| mLeft = expression mOp = LAND mRight = expression																							# landExpression
	| mLeft = expression mOp = LOR mRight = expression																							# lorExpression
	| mCond = expression mInterr = INTERR mThen = expression mCol = COLON mElse = expression													# conditionalExpression
;

typeMemberAccessExpressionSource
	: mType1 = predefinedType | mType2 = aliasGenericName
;

// Arrays
arrayRankSpecifier
	: mBracketO = OPEN_BRACKET mCommas += COMMA* mBracketC = CLOSE_BRACKET
;

arraySizeSpecifier
	: mBracketO = OPEN_BRACKET mRanks += expression (mCommas += COMMA mSizes += expression)* mBracketC = CLOSE_BRACKET
;

arrayInitializer
	: mBraceO = OPEN_BRACE (mElements += variableInitializer (mCommas += COMMA mElements += variableInitializer)* mCommas += COMMA?)? mBraceC = CLOSE_BRACE
;

// Statements

block
	: mBraceO = OPEN_BRACE mStatements += statement* mBraceC = CLOSE_BRACE
;

statement
	: mTgt = expression mOp = (ASSIGN | ASSIGN_ADD | ASSIGN_AND | ASSIGN_DIV | ASSIGN_LSH | ASSIGN_MOD | ASSIGN_MUL | ASSIGN_NULLC | ASSIGN_OR | ASSIGN_RSH | ASSIGN_SUB | ASSIGN_XOR) mSrc = expression mScol = SEMICOLON	# assignmentStatement
	| mExpr = expression mSCol = SEMICOLON																																													# expressionStatement
	| mSrc = variableDeclaration mSCol = SEMICOLON																																											# variableDeclarationStatement
	| mGlobal = GLOBAL mSrc = globalDeclaration mSCol = SEMICOLON																																							# globalDeclarationStatement
	| mIf = IF mParenO = OPEN_PARENS mCond = expression mParenC = CLOSE_PARENS mThenBody = statement mElse = elseClause?																									# ifStatement
	| mReturn = RETURN mSrc = expression mSCol = SEMICOLON																																									# returnStatement
;

elseClause
	: mElse = ELSE mBody = statement
;

implicitGlobalDeclarationStatement
	: mSrc = globalDeclaration mCol = SEMICOLON
;

typeOrVar
	: mType = type # explicityVariableType | mVar = VAR # inferredVariableType
;

variableDeclarator
	: mTgt = name (mOp = ASSIGN mSrc = variableInitializer)?
;

variableInitializer
	: mSrc = expression # expressionVariableInitializer | mSrc = arrayInitializer # arrayVariableInitializer
;

variableDeclaration
	: mType = typeOrVar mDeclarators += variableDeclarator (mCommas += COMMA mDeclarators += variableDeclarator)*
;

globalDeclaration
	: mType = typeOrVar mDeclarators += variableDeclarator (mCommas += COMMA mDeclarators += variableDeclarator)*
;

// Namespaces

usingDirective
	: mUsing = USING mAlias = nameEquals mName = name mSCol = SEMICOLON # UsingAlias | mUsing = USING mStatic = STATIC? mName = qualifiedAliasGenericName mSCol = SEMICOLON # Using
;

nameEquals
	: mName = name mEq = ASSIGN
;

name
	: mName = (IDENTIFIER | GLOBALS | STATIC | USING)
;