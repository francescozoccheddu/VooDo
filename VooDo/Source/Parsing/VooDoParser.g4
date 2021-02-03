parser grammar VooDoParser
;

options
{
	tokenVocab = VooDoLexer;
}

// Entry point

script
	: mExterns = externAliasDirective* mUsings = usingDirective* mBody = scriptBody EOF
;

inlineScript
	: externAliasDirective* mUsings = usingDirective* mBody = inlineScriptBody EOF
;

scriptBody
	: mGlobals = globalsBlock? mStatements += statement*
;

inlineScriptBody
	: mGlobals = globalsBlock? mStatements += statement* mReturn = expression
;

globalsBlock
	: mGlobals=GLOBALS mBraceO=OPEN_BRACE mStatements += implicitGlobalDeclarationStatement* mBraceC=CLOSE_BRACE
;

// Basic concepts

genericName
	: mName = name mTypeArguments = typeArgumentList?
;

aliasGenericName
	: (mAlias = name mDCol=DCOLON)? mName = genericName
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
	: mArguments += argument (mCommas += COMMA mArguments += argument)*
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
	: mLiteral = literal																								# literalExpression
	| mKeyword = (NULL | TRUE | FALSE)																					# keywordLiteralExpression
	| mOp = CONTROLLEROF mName = name																					# controllerofExpression
	| mName = name																										# nameExpression
	| mParenO = OPEN_PARENS mExpr = expression mParenC = CLOSE_PARENS													# parensExpression
	| mParenO = OPEN_PARENS mElements += expression (mCommas = COMMA mElements += expression)+ mParenC = CLOSE_PARENS	# tupleExpression
	| mDefault = DEFAULT (mParenO = OPEN_PARENS mType = type mParenC = CLOSE_PARENS)?									# defaultExpression
	| mOp = NAMEOF mParenO = OPEN_PARENS (mParts += name DOT)* mParts += name mParenC = CLOSE_PARENS					# nameofExpression
	| mSrc = expression mNullable = INTERR? mDot = DOT mMemb = genericName												# memberAccessExpression
	| mSrc = typeMemberAccessExpressionSource mDot = DOT mMemb = genericName											# typeMemberAccessExpression
	| mSrc = expression mNullable = INTERR? mArgList = bracketedArgumentList											# elementAccessExpression
	| mOp = PLUS mSrc = expression																						# unaryPlusExpression
	| mOp = MINUS mSrc = expression																						# unaryMinusExpression
	| mOp = NOT mSrc = expression																						# notExpression
	| mOp = BCOMPL mSrc = expression																					# bcomplExpression
	| mParenO = OPEN_PARENS mType = type mParenC = CLOSE_PARENS mSrc = expression										# castExpression
	| mLeft = expression mOp = (MUL | DIV | MOD) mRight = expression													# multiplicativeExpressionGroup
	| mLeft = expression mOp = (PLUS | MINUS) mRight = expression														# additiveExpressionGroup
	| mLeft = expression mOp = (LSH | RSH) mRight = expression															# shiftExpressionGroup
	| mLeft = expression mOp = (LT | GT | LE | GE) mRight = expression													# comparisonExpressionGroup
	| mLeft = expression mOp = (EQ | NEQ) mRight = expression															# equalityExpressionGroup
	| mLeft = expression mOp = AND mRight = expression																	# andExpression
	| mLeft = expression mOp = XOR mRight = expression																	# xorExpression
	| mLeft = expression mOp = OR mRight = expression																	# orExpression
	| mLeft = expression mOp = LAND mRight = expression																	# landExpression
	| mLeft = expression mOp = LOR mRight = expression																	# lorExpression
	| mSrc = expression mOp = NULLC mElse = expressionOrThrow															# nullcExpression
	| mCond = expression mInterr = INTERR mThen = expressionOrThrow mCol = COLON mElse = expressionOrThrow				# conditionalExpression
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
	: mBracketO = OPEN_BRACKET mCommas += COMMA* mBracketC = CLOSE_BRACKET
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
	| mConst = CONST mSrc = constantDeclaration mSCol = SEMICOLON																																							# constantDeclarationStatement
	| mIf = IF mParenO = OPEN_PARENS mCond = expression mParenC = CLOSE_PARENS mThenBody = statement mElse = elseClause?																									# ifStatement
	| mReturn = RETURN mSrc = expression mSCol = SEMICOLON																																									# returnStatement
	| mThrow = THROW mSrc = expression? mSCol = SEMICOLON																																									# throwStatement
	| mTry = TRY mBlock = block (mCatches = catchClauseList mFinally = finallyClause? | mFinally = finallyClause)																											# tryStatement
;

elseClause
	: mElse = ELSE mBody = statement
;

catchClauseList
	: mClauses += specificCatchClause (mClauses += specificCatchClause)* mLastClause = generalCatchClause? # multiCatch | mClause = generalCatchClause # allCatch
;

specificCatchClause
	: mCatch = CATCH mParenO = OPEN_PARENS mType = classType mName = name? mParenc = CLOSE_PARENS mBlock = block
;

generalCatchClause
	: mCatch = CATCH mBlock = block
;

finallyClause
	: mFinally = FINALLY mBlock = block
;

implicitGlobalDeclarationStatement
	: mSrc = globalDeclaration SEMICOLON
;

typeOrVar
	: mType = type # explicityVariableType | VAR # inferredVariableType
;

constantDeclarator
	: mTgt = name mOp = ASSIGN mSrc = expression
;

variableDeclarator
	: mTgt = name (mOp = ASSIGN mSrc = variableInitializer)?
;

variableInitializer
	: mSrc = expression # expressionVariableInitializer | mSrc = arrayInitializer # arrayVariableInitializer
;

constantDeclaration
	: mType = typeOrVar mDeclarators += constantDeclarator (mCommas += COMMA mDeclarators += constantDeclarator)*
;

variableDeclaration
	: mType = typeOrVar mDeclarators += variableDeclarator (mCommas += COMMA mDeclarators += variableDeclarator)*
;

globalDeclaration
	: mType = typeOrVar mDeclarators += variableDeclarator (mCommas += COMMA mDeclarators += variableDeclarator)*
;

// Namespaces

externAliasDirective
	: mExtern = EXTERN mAlias = ALIAS mName = name mSCol = SEMICOLON
;

usingDirective
	: USING mName = name SEMICOLON # UsingAlias | USING mStatic = STATIC? mName = qualifiedAliasGenericName SEMICOLON # Using
;

literal
	: mSrc = (REGULAR_STRING | VERBATIUM_STRING)	# stringLiteral
	| mSrc = INTEGER_LITERAL						# integerLiteral
	| mSrc = HEX_INTEGER_LITERAL					# hexIntegerLiteral
	| mSrc = BIN_INTEGER_LITERAL					# binIntegerLiteral
	| mSrc = REAL_LITERAL							# realLiteral
	| mSrc = CHAR_LITERAL							# charLiteral
;

name
	: mName = (IDENTIFIER | ALIAS | EXTERN | GLOBALS | STATIC)
;