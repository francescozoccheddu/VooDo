
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Parsing.Generated;
using VooDo.Transformation;
using VooDo.Utils;

namespace VooDo.Parsing
{

    using PH = ParserHelper;
    using SF = SyntaxFactory;
    using SK = SyntaxKind;

    internal sealed class Visitor : VooDoParserBaseVisitor<object>
    {

        private TNode Get<TNode>(ParserRuleContext _context)
            => (TNode) Visit(_context);

        private IEnumerable<TNode> Get<TNode>(IEnumerable<ParserRuleContext> _context)
            => _context.Select(Get<TNode>);

        private TNode TryGet<TNode>(ParserRuleContext _context) where TNode : class
            => _context != null ? Get<TNode>(_context) : null;

        public override object Visit(IParseTree _tree)
        {
            object result = base.Visit(_tree);
            if (_tree is ParserRuleContext context)
            {
                if (result is SyntaxNode node)
                {
                    result = node.From(context);
                }
                else if (result is SyntaxToken token)
                {
                    result = token.From(context);
                }
            }
            return result;
        }

        private static IEnumerable<SyntaxNodeOrToken> Interleave(IEnumerable<SyntaxNode> _nodes, IEnumerable<SyntaxToken> _tokens)
        {
            IEnumerator<SyntaxNode> nodeEnumerator = _nodes.GetEnumerator();
            IEnumerator<SyntaxToken> tokensEnumerator = _tokens.GetEnumerator();
            bool hasNodes = true, hasTokens = true;
            while ((hasNodes = nodeEnumerator.MoveNext()) | (hasTokens = tokensEnumerator.MoveNext()))
            {
                if (hasNodes)
                    yield return nodeEnumerator.Current;
                if (hasTokens)
                    yield return tokensEnumerator.Current;
            }
        }

        #region Marked

        private static SyntaxToken Tk(SK _kind, IToken _token) => SF.Token(_kind).From(_token);
        private static SyntaxToken Tk(SK _kind, ParserRuleContext _token) => SF.Token(_kind).From(_token);
        private static SyntaxToken Tk(IToken _token) => Tk(PH.TokenKind(_token), _token);
        private static SyntaxToken NoTk(IToken _token) => Tk(SK.None, _token);
        private static SyntaxToken NoTk(ParserRuleContext _token) => Tk(SK.None, _token);
        private IdentifierNameSyntax IdentifierName(VooDoParser.NameContext _token) => SF.IdentifierName(Get<SyntaxToken>(_token)).From(_token);
        private VariableDesignationSyntax VariableDesignation(VooDoParser.NameContext _name)
            => (_name.mName.Text == "_"
            ? SF.DiscardDesignation(Tk(SK.UnderscoreToken, _name))
            : (VariableDesignationSyntax) SF.SingleVariableDesignation(Get<SyntaxToken>(_name))).From(_name);

        #endregion

        #region Unmarked

        private SeparatedSyntaxList<TNode> CommaSeparatedList<TNode>(IEnumerable<TNode> _nodes, IEnumerable<IToken> _commas) where TNode : SyntaxNode
        {
            SyntaxToken[] commaTokens = _commas.Select(_t => Tk(_t)).ToArray();
            return SF.SeparatedList<TNode>(Interleave(_nodes, commaTokens));
        }

        private PrefixUnaryExpressionSyntax UnaryExpression(IToken _op, VooDoParser.ExpressionContext _src)
        {
            SK kind = PH.UnaryExpressionKind(_op);
            return SF.PrefixUnaryExpression(kind, Tk(_op), Get<ExpressionSyntax>(_src));
        }

        private BinaryExpressionSyntax BinaryExpression(IToken _op, VooDoParser.ExpressionContext _left, ParserRuleContext _right)
        {
            SK kind = PH.BinaryExpressionKind(_op);
            return SF.BinaryExpression(kind, Get<ExpressionSyntax>(_left), Tk(_op), Get<ExpressionSyntax>(_right));
        }

        private ExpressionStatementSyntax AssignmentStatement(IToken _op, VooDoParser.ExpressionContext _left, VooDoParser.ExpressionContext _right, ParserRuleContext _assignment, IToken _semicolon)
        {
            SK kind = PH.AssignmentExpressionKind(_op);
            AssignmentExpressionSyntax assignment = SF.AssignmentExpression(kind, Get<ExpressionSyntax>(_left), Tk(_op), Get<ExpressionSyntax>(_right)).From(_assignment);
            return SF.ExpressionStatement(assignment, Tk(_semicolon));
        }

        private VariableDeclarationSyntax VariableDeclaration(VooDoParser.TypeOrVarContext _type, IEnumerable<ParserRuleContext> _declarators, IEnumerable<IToken> _commas)
            => SF.VariableDeclaration(Get<TypeSyntax>(_type), CommaSeparatedList(Get<VariableDeclaratorSyntax>(_declarators), _commas));

        private VariableDeclaratorSyntax VariableDeclarator(VooDoParser.NameContext _name, IToken _assign, ParserRuleContext _initializer, ParserRuleContext _declarator)
            => SF.VariableDeclarator(Get<SyntaxToken>(_name), null, _assign != null ? SF.EqualsValueClause(Tk(_assign), TryGet<ExpressionSyntax>(_initializer)).From(_declarator) : null);

        private object Variant(params ParserRuleContext[] _rules)
            => Visit(_rules.Where(_r => _r != null).Single());

        #endregion

        public override object VisitName([NotNull] VooDoParser.NameContext _c)
            => SF.Identifier(_c.mName.Text);
        public override object VisitKeywordLiteralExpression([NotNull] VooDoParser.KeywordLiteralExpressionContext _c)
            => SF.LiteralExpression(PH.LiteralExpressionKind(_c.mKeyword), Tk(_c.mKeyword));
        public override object VisitLiteralExpression([NotNull] VooDoParser.LiteralExpressionContext _c)
            => (LiteralExpressionSyntax) SF.ParseExpression(_c.mSrc.Text).From(_c, true);
        public override object VisitAdditiveExpressionGroup([NotNull] VooDoParser.AdditiveExpressionGroupContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitAliasGenericName([NotNull] VooDoParser.AliasGenericNameContext _c)
            => _c.mAlias != null ? SF.AliasQualifiedName(IdentifierName(_c.mAlias), Tk(_c.mDCol), Get<SimpleNameSyntax>(_c.mName)) : (object) Get<SimpleNameSyntax>(_c.mName);
        public override object VisitAndExpression([NotNull] VooDoParser.AndExpressionContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitArgument([NotNull] VooDoParser.ArgumentContext _c)
        {
            NameColonSyntax param = _c.mParam != null ? Get<NameColonSyntax>(_c.mParam) : null;
            ExpressionSyntax expr = _c.mDeclExpr != null ? Get<DeclarationExpressionSyntax>(_c.mDeclExpr) : Get<ExpressionSyntax>(_c.mExpr);
            SyntaxToken refKind = _c.mKind != null ? Tk(_c.mKind) : NoTk(_c);
            return SF.Argument(param, refKind, expr);
        }
        public override object VisitNameColon([NotNull] VooDoParser.NameColonContext _c)
            => SF.NameColon(IdentifierName(_c.mName), Tk(_c.mColon));
        public override object VisitDeclarationExpression([NotNull] VooDoParser.DeclarationExpressionContext _c)
            => SF.DeclarationExpression(Get<TypeSyntax>(_c.mType), VariableDesignation(_c.mName));
        public override object VisitArgumentList([NotNull] VooDoParser.ArgumentListContext _c)
            => SF.ArgumentList(CommaSeparatedList(Get<ArgumentSyntax>(_c._mArguments), _c._mCommas));
        public override object VisitArrayInitializer([NotNull] VooDoParser.ArrayInitializerContext _c)
            => SF.InitializerExpression(SK.ArrayInitializerExpression, Tk(_c.mBraceO), CommaSeparatedList(Get<ExpressionSyntax>(_c._mElements), _c._mCommas), Tk(_c.mBraceC));
        public override object VisitArrayRankSpecifier([NotNull] VooDoParser.ArrayRankSpecifierContext _c)
        {
            OmittedArraySizeExpressionSyntax[] sizes = _c._mCommas.Count > 0
                ? _c._mCommas
                        .Concat(_c._mCommas.TakeLast(1))
                        .Select(_t => SF.OmittedArraySizeExpression(Tk(SK.OmittedArraySizeExpressionToken, _t)).From(_t))
                        .ToArray()
                : (new[] { SF.OmittedArraySizeExpression(Tk(SK.OmittedArraySizeExpressionToken, _c)).From(_c) });
            return SF.ArrayRankSpecifier(Tk(_c.mBracketO), CommaSeparatedList<ExpressionSyntax>(sizes, _c._mCommas), Tk(_c.mBracketC));
        }
        public override object VisitArrayVariableInitializer([NotNull] VooDoParser.ArrayVariableInitializerContext _c)
            => Get<InitializerExpressionSyntax>(_c.mSrc);
        public override object VisitAssignmentStatement([NotNull] VooDoParser.AssignmentStatementContext _c)
            => AssignmentStatement(_c.mOp, _c.mTgt, _c.mSrc, _c, _c.mScol);
        public override object VisitBcomplExpression([NotNull] VooDoParser.BcomplExpressionContext _c)
            => UnaryExpression(_c.mOp, _c.mSrc);
        public override object VisitBlock([NotNull] VooDoParser.BlockContext _c)
            => SF.Block(Tk(_c.mBraceO), SF.List(Get<StatementSyntax>(_c._mStatements)), Tk(_c.mBraceC));
        public override object VisitElementAccessExpression([NotNull] VooDoParser.ElementAccessExpressionContext _c)
        {
            ExpressionSyntax source = Get<ExpressionSyntax>(_c.mSrc);
            BracketedArgumentListSyntax arguments = Get<BracketedArgumentListSyntax>(_c.mArgList);
            return _c.mNullable != null
            ? SF.ConditionalAccessExpression(source, Tk(_c.mNullable), SF.ElementBindingExpression(arguments).From(_c))
            : (object) SF.ElementAccessExpression(source, arguments);
        }
        public override object VisitBracketedArgumentList([NotNull] VooDoParser.BracketedArgumentListContext _c)
            => SF.BracketedArgumentList(Tk(_c.mBracketO), CommaSeparatedList(Get<ArgumentSyntax>(_c._mArguments), _c._mCommas), Tk(_c.mBracketC));
        public override object VisitCastExpression([NotNull] VooDoParser.CastExpressionContext _c)
            => SF.CastExpression(Tk(_c.mParenO), Get<TypeSyntax>(_c.mType), Tk(_c.mParenC), Get<ExpressionSyntax>(_c.mSrc));
        public override object VisitClassType([NotNull] VooDoParser.ClassTypeContext _c)
            => Variant(_c.mType1, _c.mType2);
        public override object VisitComparisonExpressionGroup([NotNull] VooDoParser.ComparisonExpressionGroupContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitConditionalExpression([NotNull] VooDoParser.ConditionalExpressionContext _c)
            => SF.ConditionalExpression(Get<ExpressionSyntax>(_c.mCond), Tk(_c.mInterr), Get<ExpressionSyntax>(_c.mThen), Tk(_c.mCol), Get<ExpressionSyntax>(_c.mElse));
        public override object VisitControllerofExpression([NotNull] VooDoParser.ControllerofExpressionContext _c)
        {
            ArgumentSyntax argument = SF.Argument(IdentifierName(_c.mName)).From(_c.mName);
            TypeSyntax globalType = SyntaxFactoryHelper.Type(typeof(Meta), Identifiers.referenceAlias);
            MemberAccessExpressionSyntax method = (MemberAccessExpressionSyntax) SyntaxFactoryHelper.Generator.MemberAccessExpression(globalType, nameof(Meta.cof));
            return SF.InvocationExpression(
                    method.From(_c.mOp, true),
                    SF.ArgumentList(
                        Tk(SK.OpenParenToken, _c),
                        SF.SingletonSeparatedList(argument),
                        Tk(SK.CloseParenToken, _c)
                    ).From(_c.mName));
        }
        public override object VisitDefaultExpression([NotNull] VooDoParser.DefaultExpressionContext _c)
            => SF.DefaultExpression(Tk(_c.mDefault), Tk(_c.mParenO), Get<TypeSyntax>(_c.mType), Tk(_c.mParenC));
        public override object VisitEqualityExpressionGroup([NotNull] VooDoParser.EqualityExpressionGroupContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitExplicityVariableType([NotNull] VooDoParser.ExplicityVariableTypeContext _c)
            => Visit(_c.mType);
        public override object VisitExpressionStatement([NotNull] VooDoParser.ExpressionStatementContext _c)
            => SF.ExpressionStatement(Get<ExpressionSyntax>(_c.mExpr), Tk(_c.mSCol));
        public override object VisitExpressionVariableInitializer([NotNull] VooDoParser.ExpressionVariableInitializerContext _c)
            => Visit(_c.mSrc);
        public override object VisitGenericName([NotNull] VooDoParser.GenericNameContext _c)
            => _c.mTypeArguments != null
            ? SF.GenericName(Get<SyntaxToken>(_c.mName), Get<TypeArgumentListSyntax>(_c.mTypeArguments))
            : (object) IdentifierName(_c.mName);
        public override object VisitGlobalDeclarationStatement([NotNull] VooDoParser.GlobalDeclarationStatementContext _c)
            => SF.LocalDeclarationStatement(SF.TokenList(), Get<VariableDeclarationSyntax>(_c.mSrc), Tk(_c.mSCol));
        public override object VisitGlobalsBlock([NotNull] VooDoParser.GlobalsBlockContext _c)
            => SF.List(Get<LocalDeclarationStatementSyntax>(_c._mStatements));
        public override object VisitIfStatement([NotNull] VooDoParser.IfStatementContext _c)
            => SF.IfStatement(Tk(_c.mIf), Tk(_c.mParenO), Get<ExpressionSyntax>(_c.mCond), Tk(_c.mParenC), Get<StatementSyntax>(_c.mThenBody), TryGet<ElseClauseSyntax>(_c.mElse));
        public override object VisitGlobalDeclaration([NotNull] VooDoParser.GlobalDeclarationContext _c)
        {
            NameSyntax globalUnboundType = (NameSyntax) SyntaxFactoryHelper.Type(typeof(Meta.Glob<>), Identifiers.referenceAlias).From(_c);
            if (_c.Parent is VooDoParser.GlobalDeclarationStatementContext declaration)
            {
                globalUnboundType = globalUnboundType.From(declaration.mGlobal);
            }
            NameSyntax globalType = SyntaxFactoryHelper.GenericType(globalUnboundType, new[] { Get<TypeSyntax>(_c.mType) }).From(_c);
            return SF.VariableDeclaration(globalType, CommaSeparatedList(Get<VariableDeclaratorSyntax>(_c._mDeclarators), _c._mCommas));
        }
        public override object VisitElseClause([NotNull] VooDoParser.ElseClauseContext _c)
            => SF.ElseClause(Tk(_c.mElse), Get<StatementSyntax>(_c.mBody));
        public override object VisitImplicitGlobalDeclarationStatement([NotNull] VooDoParser.ImplicitGlobalDeclarationStatementContext _c)
            => SF.LocalDeclarationStatement(SF.TokenList(), Get<VariableDeclarationSyntax>(_c.mSrc), Tk(_c.mCol));
        public override object VisitIndexerArgument([NotNull] VooDoParser.IndexerArgumentContext _c)
            => SF.Argument(TryGet<NameColonSyntax>(_c.mParam), _c.mIn != null ? Tk(_c.mIn) : NoTk(_c), Get<ExpressionSyntax>(_c.mExpr));
        public override object VisitInferredVariableType([NotNull] VooDoParser.InferredVariableTypeContext _c)
            => SF.IdentifierName(SF.Identifier(SF.TriviaList(), SK.VarKeyword, "var", "var", SF.TriviaList()).From(_c.mVar));
        public override object VisitLandExpression([NotNull] VooDoParser.LandExpressionContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitLorExpression([NotNull] VooDoParser.LorExpressionContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitMemberAccessExpression([NotNull] VooDoParser.MemberAccessExpressionContext _c)
            => SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, Get<ExpressionSyntax>(_c.mSrc), Tk(_c.mDot), Get<SimpleNameSyntax>(_c.mMemb));
        public override object VisitMultiplicativeExpressionGroup([NotNull] VooDoParser.MultiplicativeExpressionGroupContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitNameExpression([NotNull] VooDoParser.NameExpressionContext _c)
            => IdentifierName(_c.mName);
        public override object VisitNonArrayType([NotNull] VooDoParser.NonArrayTypeContext _c)
            => Variant(_c.mType1, _c.mType2, _c.mType3);
        public override object VisitNotExpression([NotNull] VooDoParser.NotExpressionContext _c)
            => UnaryExpression(_c.mOp, _c.mSrc);
        public override object VisitOrExpression([NotNull] VooDoParser.OrExpressionContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitParensExpression([NotNull] VooDoParser.ParensExpressionContext _c)
            => SF.ParenthesizedExpression(Tk(_c.mParenO), Get<ExpressionSyntax>(_c.mExpr), Tk(_c.mParenC));
        public override object VisitPredefinedClassType([NotNull] VooDoParser.PredefinedClassTypeContext _c)
            => SF.PredefinedType(Tk(_c.mType));
        public override object VisitPredefinedStructType([NotNull] VooDoParser.PredefinedStructTypeContext _c)
            => SF.PredefinedType(Tk(_c.mType));
        public override object VisitPredefinedType([NotNull] VooDoParser.PredefinedTypeContext _c)
            => Variant(_c.mType1, _c.mType2);
        public override object VisitReturnStatement([NotNull] VooDoParser.ReturnStatementContext _c)
            => SF.ReturnStatement(Tk(_c.mReturn), Get<ExpressionSyntax>(_c.mSrc), Tk(_c.mSCol));
        public override object VisitScript([NotNull] VooDoParser.ScriptContext _c)
            => SF.CompilationUnit(SF.List<ExternAliasDirectiveSyntax>(), SF.List(Get<UsingDirectiveSyntax>(_c._mUsings)), SF.List<AttributeListSyntax>(), Get<SyntaxList<MemberDeclarationSyntax>>(_c.mBody));
        public override object VisitShiftExpressionGroup([NotNull] VooDoParser.ShiftExpressionGroupContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitTupleExpression([NotNull] VooDoParser.TupleExpressionContext _c)
            => SF.TupleExpression(Tk(_c.mParenO), CommaSeparatedList(Get<ArgumentSyntax>(_c._mElements), _c._mCommas), Tk(_c.mParenC));
        public override object VisitTupleType([NotNull] VooDoParser.TupleTypeContext _c)
            => SF.TupleType(Tk(_c.mParenO), CommaSeparatedList(Get<TupleElementSyntax>(_c._mElements), _c._mCommas), Tk(_c.mParenC));
        public override object VisitTupleTypeElement([NotNull] VooDoParser.TupleTypeElementContext _c)
            => SF.TupleElement(Get<TypeSyntax>(_c.mType), _c.mName != null ? Get<SyntaxToken>(_c.mName) : NoTk(_c));
        public override object VisitType([NotNull] VooDoParser.TypeContext _c)
            => _c._mRanks.Count > 0 ? SF.ArrayType(Get<TypeSyntax>(_c.mType), SF.List(Get<ArrayRankSpecifierSyntax>(_c._mRanks))) : Visit(_c.mType);
        public override object VisitTypeArgumentList([NotNull] VooDoParser.TypeArgumentListContext _c)
            => SF.TypeArgumentList(Tk(_c.mLt), CommaSeparatedList(Get<TypeSyntax>(_c._mTypes), _c._mCommas), Tk(_c.mGt));
        public override object VisitTypeMemberAccessExpression([NotNull] VooDoParser.TypeMemberAccessExpressionContext _c)
            => SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, Get<ExpressionSyntax>(_c.mSrc), Tk(_c.mDot), Get<SimpleNameSyntax>(_c.mMemb));
        public override object VisitTypeMemberAccessExpressionSource([NotNull] VooDoParser.TypeMemberAccessExpressionSourceContext _c)
            => Variant(_c.mType1, _c.mType2);
        public override object VisitUnaryMinusExpression([NotNull] VooDoParser.UnaryMinusExpressionContext _c)
            => UnaryExpression(_c.mOp, _c.mSrc);
        public override object VisitUnaryPlusExpression([NotNull] VooDoParser.UnaryPlusExpressionContext _c)
            => UnaryExpression(_c.mOp, _c.mSrc);
        public override object VisitUsing([NotNull] VooDoParser.UsingContext _c)
            => SF.UsingDirective(Tk(_c.mUsing), _c.mStatic != null ? Tk(_c.mStatic) : NoTk(_c), null, Get<NameSyntax>(_c.mName), Tk(_c.mSCol));
        public override object VisitUsingAlias([NotNull] VooDoParser.UsingAliasContext _c)
            => SF.UsingDirective(Tk(_c.mUsing), NoTk(_c), Get<NameEqualsSyntax>(_c.mAlias), Get<NameSyntax>(_c.mName), Tk(_c.mSCol));
        public override object VisitNameEquals([NotNull] VooDoParser.NameEqualsContext _c)
            => SF.NameEquals(IdentifierName(_c.mName), Tk(_c.mEq));
        public override object VisitVariableDeclaration([NotNull] VooDoParser.VariableDeclarationContext _c)
            => VariableDeclaration(_c.mType, _c._mDeclarators, _c._mCommas);
        public override object VisitVariableDeclarationStatement([NotNull] VooDoParser.VariableDeclarationStatementContext _c)
            => SF.LocalDeclarationStatement(SF.TokenList(), Get<VariableDeclarationSyntax>(_c.mSrc), Tk(_c.mSCol));
        public override object VisitVariableDeclarator([NotNull] VooDoParser.VariableDeclaratorContext _c)
            => VariableDeclarator(_c.mTgt, _c.mOp, _c.mSrc, _c);
        public override object VisitXorExpression([NotNull] VooDoParser.XorExpressionContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitInlineScript([NotNull] VooDoParser.InlineScriptContext _c)
            => SF.CompilationUnit(SF.List<ExternAliasDirectiveSyntax>(), SF.List<UsingDirectiveSyntax>(), SF.List<AttributeListSyntax>(),
                SF.SingletonList<MemberDeclarationSyntax>(
                    SF.GlobalStatement(
                        SF.ReturnStatement(
                            Tk(SK.ReturnKeyword, _c),
                            Get<ExpressionSyntax>(_c.mSrc),
                            SF.Token(SK.SemicolonToken)
                            .From(_c)).From(_c)).From(_c)));
        public override object VisitScriptBody([NotNull] VooDoParser.ScriptBodyContext _c)
            => SF.List<MemberDeclarationSyntax>(
                TryGet<IEnumerable<StatementSyntax>>(_c.mGlobals)
                .EmptyIfNull()
                .Concat(Get<StatementSyntax>(_c._mStatements))
                .Select(_s => SF.GlobalStatement(_s).From(_c)));
        public override object VisitQualifiedName([NotNull] VooDoParser.QualifiedNameContext _c)
            => SF.QualifiedName(Get<NameSyntax>(_c.mLeft), Get<SimpleNameSyntax>(_c.mRight));
        public override object VisitQualifiedGenericNamePass([NotNull] VooDoParser.QualifiedGenericNamePassContext _c)
            => Visit(_c.mSrc);
        public override object VisitNullableType([NotNull] VooDoParser.NullableTypeContext _c)
            => _c.mQuest != null ? SF.NullableType(Get<TypeSyntax>(_c.mType), Tk(_c.mQuest)) : Visit(_c.mType);
        public override object VisitIsExpression([NotNull] VooDoParser.IsExpressionContext _c)
            => _c.mName != null
            ? SF.IsPatternExpression(Get<ExpressionSyntax>(_c.mSrc), Tk(_c.mOp), SF.DeclarationPattern(
                Get<TypeSyntax>(_c.mType), VariableDesignation(_c.mName)).From(_c))
            : (object) BinaryExpression(_c.mOp, _c.mSrc, _c.mType);
        public override object VisitAsExpression([NotNull] VooDoParser.AsExpressionContext _c)
            => BinaryExpression(_c.mOp, _c.mSrc, _c.mType);
        public override object VisitInvocationExpression([NotNull] VooDoParser.InvocationExpressionContext _c)
            => SF.InvocationExpression(Get<ExpressionSyntax>(_c.mSrc), Get<ArgumentListSyntax>(_c.mArgumentList));
        public override object VisitObjectCreationExpression([NotNull] VooDoParser.ObjectCreationExpressionContext _c)
            => SF.ObjectCreationExpression(Tk(_c.mNew), Get<TypeSyntax>(_c.mType), Get<ArgumentListSyntax>(_c.mArgumentList), null);
        public override object VisitArrayCreationExpression([NotNull] VooDoParser.ArrayCreationExpressionContext _c)
            => SF.ArrayCreationExpression(Tk(_c.mNew), Get<ArrayTypeSyntax>(_c.mType), Get<InitializerExpressionSyntax>(_c.mInitializer));
        public override object VisitArraySizeSpecifier([NotNull] VooDoParser.ArraySizeSpecifierContext _c)
            => SF.ArrayRankSpecifier(Tk(_c.mBracketO), CommaSeparatedList(Get<ExpressionSyntax>(_c._mSizes), _c._mCommas), Tk(_c.mBracketC));
        public override object VisitTypeWithSize([NotNull] VooDoParser.TypeWithSizeContext _c)
            => SF.ArrayType(Get<TypeSyntax>(_c.mType), SF.List(Get<ArrayRankSpecifierSyntax>(new ParserRuleContext[] { _c.mType }.Concat(_c._mRanks))));
        public override object VisitImplicitObjectCreationExpression([NotNull] VooDoParser.ImplicitObjectCreationExpressionContext _c)
            => SF.ImplicitObjectCreationExpression(Tk(_c.mNew), Get<ArgumentListSyntax>(_c.mArgumentList), null);
        public override object VisitGlobExpression([NotNull] VooDoParser.GlobExpressionContext _c)
        {
            List<SyntaxNodeOrToken> argList = new List<SyntaxNodeOrToken>
            {
                SF.Argument(Get<ExpressionSyntax>(_c.mContr))
            };
            if (_c.mInitializer != null)
            {
                argList.Add(Tk(SK.CommaToken, _c.mInit));
                argList.Add(SF.Argument(Get<ExpressionSyntax>(_c.mInitializer)));
            }
            TypeSyntax globalType = SyntaxFactoryHelper.Type(typeof(Meta), Identifiers.referenceAlias);
            MemberAccessExpressionSyntax method = (MemberAccessExpressionSyntax) SyntaxFactoryHelper.Generator.MemberAccessExpression(globalType, nameof(Meta.gexpr));
            return SF.InvocationExpression(method.From(_c, true), SF.ArgumentList(Tk(SK.OpenParenToken, _c), SF.SeparatedList<ArgumentSyntax>(argList), Tk(SK.CloseParenToken, _c)).From(_c));
        }
        public override object VisitAnyScript([NotNull] VooDoParser.AnyScriptContext _c)
            => Variant(_c.mScript1, _c.mScript2);
        public override object VisitBlockStatement([NotNull] VooDoParser.BlockStatementContext _c) => Visit(_c.mBlock);

    }

}
