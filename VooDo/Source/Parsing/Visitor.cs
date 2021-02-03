
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Parsing.Generated;
using VooDo.Transformation.Meta;
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

        public override object Visit(IParseTree _tree)
        {
            object result = base.Visit(_tree);
            if (_tree is ParserRuleContext context)
            {
                if (result is SyntaxNode node)
                {
                    result = node.Mark(context);
                }
                else if (result is SyntaxToken token)
                {
                    result = token.Mark(context);
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

        private static SyntaxToken Token(SK _kind, IToken _token) => SF.Token(_kind).Mark(_token);
        private static SyntaxToken Semicolon(IToken _token) => Token(SK.SemicolonToken, _token);
        private static SyntaxToken OpenBrace(IToken _token) => Token(SK.OpenBraceToken, _token);
        private static SyntaxToken CloseBrace(IToken _token) => Token(SK.CloseBraceToken, _token);
        private static SyntaxToken OpenBracket(IToken _token) => Token(SK.OpenBracketToken, _token);
        private static SyntaxToken CloseBracket(IToken _token) => Token(SK.CloseBracketToken, _token);
        private static SyntaxToken OpenParen(IToken _token) => Token(SK.OpenParenToken, _token);
        private static SyntaxToken CloseParen(IToken _token) => Token(SK.CloseParenToken, _token);
        private IdentifierNameSyntax IdentifierName(VooDoParser.NameContext _token) => SF.IdentifierName(Get<SyntaxToken>(_token)).Mark(_token);


        #endregion

        #region Unmarked

        private SeparatedSyntaxList<TNode> CommaSeparatedList<TNode>(IEnumerable<TNode> _nodes, IEnumerable<IToken> _commas) where TNode : SyntaxNode
        {
            SyntaxToken[] commaTokens = _commas.Select(_t => Token(SK.CommaToken, _t)).ToArray();
            return SF.SeparatedList<TNode>(Interleave(_nodes, commaTokens));
        }

        private PrefixUnaryExpressionSyntax UnaryExpression(IToken _op, VooDoParser.ExpressionContext _src)
        {
            SK kind = PH.UnaryExpressionKind(_op);
            return SF.PrefixUnaryExpression(kind, Token(kind, _op), Get<ExpressionSyntax>(_src));
        }

        private BinaryExpressionSyntax BinaryExpression(IToken _op, VooDoParser.ExpressionContext _left, VooDoParser.ExpressionContext _right)
        {
            SK kind = PH.BinaryExpressionKind(_op);
            return SF.BinaryExpression(kind, Get<ExpressionSyntax>(_left), Token(kind, _op), Get<ExpressionSyntax>(_right));
        }

        private ExpressionStatementSyntax AssignmentStatement(IToken _op, VooDoParser.ExpressionContext _left, VooDoParser.ExpressionContext _right, ParserRuleContext _assignment, IToken _semicolon)
        {
            SK kind = PH.AssignmentExpressionKind(_op);
            AssignmentExpressionSyntax assignment = SF.AssignmentExpression(kind, Get<ExpressionSyntax>(_left), Token(kind, _op), Get<ExpressionSyntax>(_right)).Mark(_assignment);
            return SF.ExpressionStatement(assignment, Semicolon(_semicolon));
        }

        private VariableDeclarationSyntax VariableDeclaration(VooDoParser.TypeOrVarContext _type, IEnumerable<ParserRuleContext> _declarators, IEnumerable<IToken> _commas)
            => SF.VariableDeclaration(Get<TypeSyntax>(_type), CommaSeparatedList(_declarators.Select(Get<VariableDeclaratorSyntax>), _commas));

        private VariableDeclaratorSyntax VariableDeclarator(VooDoParser.NameContext _name, IToken _assign, ParserRuleContext _initializer, ParserRuleContext _declarator)
            => SF.VariableDeclarator(Get<SyntaxToken>(_name), null, SF.EqualsValueClause(Token(SK.EqualsToken, _assign), Get<ExpressionSyntax>(_initializer)).Mark(_declarator));

        private object Variant(params ParserRuleContext[] _rules)
            => Visit(_rules.Where(_r => _r != null).Single());

        #endregion

        public override object VisitName([NotNull] VooDoParser.NameContext _c)
            => SF.Identifier(_c.mName.Text);
        public override object VisitBinIntegerLiteral([NotNull] VooDoParser.BinIntegerLiteralContext _c)
            => SF.Literal(_c.mSrc.Text, PH.BinaryIntegerLiteral(_c.mSrc.Text));
        public override object VisitCharLiteral([NotNull] VooDoParser.CharLiteralContext _c)
            => SF.Literal(_c.mSrc.Text, PH.CharLiteral(_c.mSrc.Text));
        public override object VisitIntegerLiteral([NotNull] VooDoParser.IntegerLiteralContext _c)
            => SF.Literal(_c.mSrc.Text, PH.DecimalOrOctalIntegerLiteral(_c.mSrc.Text));
        public override object VisitKeywordLiteralExpression([NotNull] VooDoParser.KeywordLiteralExpressionContext _c)
        {
            SK kind = PH.KeywordKind(_c.mKeyword);
            return SF.LiteralExpression(kind, Token(kind, _c.mKeyword));
        }
        public override object VisitHexIntegerLiteral([NotNull] VooDoParser.HexIntegerLiteralContext _c)
            => SF.Literal(_c.mSrc.Text, PH.HexadecimalIntegerLiteral(_c.mSrc.Text));
        public override object VisitRealLiteral([NotNull] VooDoParser.RealLiteralContext _c)
            => SF.Literal(_c.mSrc.Text, PH.RealLiteral(_c.mSrc.Text));
        public override object VisitStringLiteral([NotNull] VooDoParser.StringLiteralContext _c)
            => SF.Literal(_c.mSrc.Text, PH.StringLiteral(_c.mSrc.Text));
        public override object VisitExternAliasDirective([NotNull] VooDoParser.ExternAliasDirectiveContext _c)
            => SF.ExternAliasDirective(Token(SK.ExternKeyword, _c.mExtern), Token(SK.AliasKeyword, _c.mAlias), Get<SyntaxToken>(_c), Semicolon(_c.mSCol));
        public override object VisitAllCatch([NotNull] VooDoParser.AllCatchContext _c)
            => SF.SingletonList(Get<CatchClauseSyntax>(_c.mClause));
        public override object VisitAdditiveExpressionGroup([NotNull] VooDoParser.AdditiveExpressionGroupContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitAliasGenericName([NotNull] VooDoParser.AliasGenericNameContext _c)
            => _c.mAlias != null ? SF.AliasQualifiedName(IdentifierName(_c.mAlias), Token(SK.ColonToken, _c.mDCol), Get<SimpleNameSyntax>(_c.mName)) : (object) Get<SimpleNameSyntax>(_c.mName);
        public override object VisitAndExpression([NotNull] VooDoParser.AndExpressionContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitArgument([NotNull] VooDoParser.ArgumentContext _c)
        {
            NameColonSyntax param = _c.mParam != null ? Get<NameColonSyntax>(_c.mParam) : null;
            ExpressionSyntax expr = _c.mDeclExpr != null ? Get<DeclarationExpressionSyntax>(_c.mDeclExpr) : Get<ExpressionSyntax>(_c.mExpr);
            SyntaxToken refKind = _c.mKind != null ? Token(PH.KeywordKind(_c.mKind), _c.mKind) : SF.Token(SK.None);
            return SF.Argument(param, refKind, expr);
        }
        public override object VisitNameColon([NotNull] VooDoParser.NameColonContext _c)
            => SF.NameColon(IdentifierName(_c.mName), Token(SK.ColonToken, _c.mColon));
        public override object VisitDeclarationExpression([NotNull] VooDoParser.DeclarationExpressionContext _c)
            => SF.DeclarationExpression(Get<TypeSyntax>(_c.mType), SF.SingleVariableDesignation(Get<SyntaxToken>(_c.mName)).Mark(_c.mName));
        public override object VisitArgumentList([NotNull] VooDoParser.ArgumentListContext _c)
            => SF.ArgumentList(CommaSeparatedList(_c._mArguments.Select(Get<ArgumentSyntax>), _c._mCommas));
        public override object VisitArrayInitializer([NotNull] VooDoParser.ArrayInitializerContext _c)
            => SF.InitializerExpression(SK.ArrayInitializerExpression, OpenBrace(_c.mBraceO), CommaSeparatedList(_c._mElements.Select(Get<ExpressionSyntax>), _c._mCommas), CloseBrace(_c.mBraceC));
        public override object VisitArrayRankSpecifier([NotNull] VooDoParser.ArrayRankSpecifierContext _c)
        {
            OmittedArraySizeExpressionSyntax[] sizes = _c._mCommas.Count > 0
                ? _c._mCommas
                        .Concat(_c._mCommas.TakeLast(1))
                        .Select(_t => SF.OmittedArraySizeExpression(Token(SK.OmittedArraySizeExpressionToken, _t)).Mark(_t))
                        .ToArray()
                : (new[] { SF.OmittedArraySizeExpression(SF.Token(SK.OmittedArraySizeExpressionToken).Mark(_c)).Mark(_c) });
            return SF.ArrayRankSpecifier(OpenBracket(_c.mBracketO), CommaSeparatedList<ExpressionSyntax>(sizes, _c._mCommas), CloseBracket(_c.mBracketC));
        }
        public override object VisitArrayVariableInitializer([NotNull] VooDoParser.ArrayVariableInitializerContext _c)
            => Get<InitializerExpressionSyntax>(_c.mSrc);
        public override object VisitAssignmentStatement([NotNull] VooDoParser.AssignmentStatementContext _c)
            => AssignmentStatement(_c.mOp, _c.mTgt, _c.mSrc, _c, _c.mScol);
        public override object VisitBcomplExpression([NotNull] VooDoParser.BcomplExpressionContext _c)
            => UnaryExpression(_c.mOp, _c.mSrc);
        public override object VisitBlock([NotNull] VooDoParser.BlockContext _c)
            => SF.Block(OpenBrace(_c.mBraceO), SF.List(_c._mStatements.Select(Get<StatementSyntax>)), CloseBrace(_c.mBraceC));
        public override object VisitElementAccessExpression([NotNull] VooDoParser.ElementAccessExpressionContext _c)
        {
            ExpressionSyntax source = Get<ExpressionSyntax>(_c.mSrc);
            BracketedArgumentListSyntax arguments = Get<BracketedArgumentListSyntax>(_c.mArgList);
            return _c.mNullable != null
            ? SF.ConditionalAccessExpression(source, Token(SK.QuestionToken, _c.mNullable), SF.ElementBindingExpression(arguments).Mark(_c))
            : (object) SF.ElementAccessExpression(source, arguments);
        }
        public override object VisitBracketedArgumentList([NotNull] VooDoParser.BracketedArgumentListContext _c)
            => SF.BracketedArgumentList(OpenBracket(_c.mBracketO), CommaSeparatedList(_c._mArguments.Select(Get<ArgumentSyntax>), _c._mCommas), CloseBracket(_c.mBracketC));
        public override object VisitCastExpression([NotNull] VooDoParser.CastExpressionContext _c)
            => SF.CastExpression(OpenParen(_c.mParenO), Get<TypeSyntax>(_c.mType), CloseParen(_c.mParenC), Get<ExpressionSyntax>(_c.mSrc));
        public override object VisitClassType([NotNull] VooDoParser.ClassTypeContext _c)
            => Variant(_c.mType1, _c.mType2);
        public override object VisitComparisonExpressionGroup([NotNull] VooDoParser.ComparisonExpressionGroupContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitConditionalExpression([NotNull] VooDoParser.ConditionalExpressionContext _c)
            => SF.ConditionalExpression(Get<ExpressionSyntax>(_c.mCond), Token(SK.QuestionToken, _c.mInterr), Get<ExpressionSyntax>(_c.mThen), Token(SK.ColonToken, _c.mCol), Get<ExpressionSyntax>(_c.mElse));
        public override object VisitConstantDeclaration([NotNull] VooDoParser.ConstantDeclarationContext _c)
            => VariableDeclaration(_c.mType, _c._mDeclarators, _c._mCommas);
        public override object VisitConstantDeclarationStatement([NotNull] VooDoParser.ConstantDeclarationStatementContext _c)
            => SF.LocalDeclarationStatement(SF.TokenList(Token(SK.ConstKeyword, _c.mConst)), Get<VariableDeclarationSyntax>(_c.mSrc), Semicolon(_c.mSCol));
        public override object VisitConstantDeclarator([NotNull] VooDoParser.ConstantDeclaratorContext _c)
            => VariableDeclarator(_c.mTgt, _c.mOp, _c.mSrc, _c);
        public override object VisitControllerofExpression([NotNull] VooDoParser.ControllerofExpressionContext _c)
        {
            ArgumentSyntax argument = SF.Argument(IdentifierName(_c.mName)).Mark(_c.mName);
            TypeSyntax globalType = SyntaxFactoryHelper.Type(typeof(Glob));
            MemberAccessExpressionSyntax method = (MemberAccessExpressionSyntax) SyntaxFactoryHelper.Generator.MemberAccessExpression(globalType, nameof(Glob.cof));
            return SF.InvocationExpression(
                    method.Mark(_c.mOp, true),
                    SF.ArgumentList(SF.SingletonSeparatedList(argument)).Mark(_c.mName));
        }
        public override object VisitDefaultExpression([NotNull] VooDoParser.DefaultExpressionContext _c)
            => _c.mType != null
            ? SF.DefaultExpression(Token(SK.DefaultKeyword, _c.mDefault), OpenParen(_c.mParenO), Get<TypeSyntax>(_c.mType), CloseParen(_c.mParenC))
            : (object) SF.LiteralExpression(SK.DefaultLiteralExpression, Token(SK.DefaultKeyword, _c.mDefault));
        public override object VisitEqualityExpressionGroup([NotNull] VooDoParser.EqualityExpressionGroupContext _c)
            => BinaryExpression(_c.mOp, _c.mLeft, _c.mRight);
        public override object VisitExplicityVariableType([NotNull] VooDoParser.ExplicityVariableTypeContext _c)
            => Visit(_c.mType);
        public override object VisitExpressionOrThrow([NotNull] VooDoParser.ExpressionOrThrowContext _c)
            => Variant(_c.mExpr1, _c.mExpr2);
        public override object VisitExpressionStatement([NotNull] VooDoParser.ExpressionStatementContext _c)
            => SF.ExpressionStatement(Get<ExpressionSyntax>(_c.mExpr), Semicolon(_c.mSCol));
        public override object VisitExpressionVariableInitializer([NotNull] VooDoParser.ExpressionVariableInitializerContext _c)
            => Visit(_c.mSrc);
        public override object VisitFinallyClause([NotNull] VooDoParser.FinallyClauseContext _c)
            => SF.FinallyClause(Token(SK.FinallyKeyword, _c.mFinally), Get<BlockSyntax>(_c.mBlock));
        public override object VisitGeneralCatchClause([NotNull] VooDoParser.GeneralCatchClauseContext _c)
            => SF.CatchClause(Token(SK.CaseKeyword, _c.mCatch), null, null, Get<BlockSyntax>(_c.mBlock));
        public override object VisitGenericName([NotNull] VooDoParser.GenericNameContext _c)
            => SF.GenericName(Get<SyntaxToken>(_c.mName), Get<TypeArgumentListSyntax>(_c.mTypeArguments));
        public override object VisitGlobalDeclarationStatement([NotNull] VooDoParser.GlobalDeclarationStatementContext _c)
            => SF.LocalDeclarationStatement(SF.TokenList(), Get<VariableDeclarationSyntax>(_c.mSrc), Semicolon(_c.mSCol));
        public override object VisitGlobalsBlock([NotNull] VooDoParser.GlobalsBlockContext _c)
            => SF.List(_c._mStatements.Select(Get<LocalDeclarationStatementSyntax>));
        public override object VisitIfStatement([NotNull] VooDoParser.IfStatementContext _c)
            => SF.IfStatement(Token(SK.IfKeyword, _c.mIf), OpenParen(_c.mParenO), Get<ExpressionSyntax>(_c.mCond), CloseParen(_c.mParenC), Get<StatementSyntax>(_c.mThenBody), Get<ElseClauseSyntax>(_c.mElse));
        public override object VisitGlobalDeclaration([NotNull] VooDoParser.GlobalDeclarationContext _c)
        {
            RuleContext parent = _c.Parent;
            IToken globalToken = null;
            while (parent != null)
            {
                if (parent is VooDoParser.GlobalsBlockContext globalsBlock)
                {
                    globalToken = globalsBlock.mGlobals;
                    break;
                }
                else if (parent is VooDoParser.GlobalDeclarationStatementContext globalDeclarationStatement)
                {
                    globalToken = globalDeclarationStatement.mGlobal;
                    break;
                }
                parent = _c.Parent;
            }
            NameSyntax globalUnboundType = (NameSyntax) SyntaxFactoryHelper.Type(typeof(Glob<>)).Mark(globalToken);
            SyntaxNode globalType = SyntaxFactoryHelper.GenericType(globalUnboundType, new[] { Get<TypeSyntax>(_c.mType) }).Mark(_c);
            return SF.VariableDeclaration(global, CommaSeparatedList(_c._mDeclarators.Select(Get<VariableDeclaratorSyntax>), _c._mCommas));
        }
        public override object VisitElseClause([NotNull] VooDoParser.ElseClauseContext _c)
            => SF.ElseClause(Token(SK.ElseKeyword, _c.mElse), Get<StatementSyntax>(_c.mBody));
        public override object VisitInlineScriptBody([NotNull] VooDoParser.InlineScriptBodyContext _c) => base.VisitInlineScriptBody(context);
        public override object VisitScriptBody([NotNull] VooDoParser.ScriptBodyContext _c) => base.VisitScriptBody(context);
        public override object VisitImplicitGlobalDeclarationStatement([NotNull] VooDoParser.ImplicitGlobalDeclarationStatementContext _c)
            => base.VisitImplicitGlobalDeclarationStatement(_c);
        public override object VisitIndexerArgument([NotNull] VooDoParser.IndexerArgumentContext _c)
            => base.VisitIndexerArgument(_c);
        public override object VisitInferredVariableType([NotNull] VooDoParser.InferredVariableTypeContext _c)
            => base.VisitInferredVariableType(_c);
        public override object VisitInlineScript([NotNull] VooDoParser.InlineScriptContext _c)
            => base.VisitInlineScript(_c);
        public override object VisitLandExpression([NotNull] VooDoParser.LandExpressionContext _c)
            => base.VisitLandExpression(_c);
        public override object VisitLiteralExpression([NotNull] VooDoParser.LiteralExpressionContext _c)
            => base.VisitLiteralExpression(_c);
        public override object VisitLorExpression([NotNull] VooDoParser.LorExpressionContext _c)
            => base.VisitLorExpression(_c);
        public override object VisitMemberAccessExpression([NotNull] VooDoParser.MemberAccessExpressionContext _c)
            => base.VisitMemberAccessExpression(_c);
        public override object VisitMultiCatch([NotNull] VooDoParser.MultiCatchContext _c)
            => base.VisitMultiCatch(_c);
        public override object VisitMultiplicativeExpressionGroup([NotNull] VooDoParser.MultiplicativeExpressionGroupContext _c)
            => base.VisitMultiplicativeExpressionGroup(_c);
        public override object VisitNameExpression([NotNull] VooDoParser.NameExpressionContext _c)
            => base.VisitNameExpression(_c);
        public override object VisitNameofExpression([NotNull] VooDoParser.NameofExpressionContext _c)
            => base.VisitNameofExpression(_c);
        public override object VisitNonArrayType([NotNull] VooDoParser.NonArrayTypeContext _c)
            => base.VisitNonArrayType(_c);
        public override object VisitNotExpression([NotNull] VooDoParser.NotExpressionContext _c)
            => base.VisitNotExpression(_c);
        public override object VisitNullcExpression([NotNull] VooDoParser.NullcExpressionContext _c)
            => base.VisitNullcExpression(_c);
        public override object VisitOrExpression([NotNull] VooDoParser.OrExpressionContext _c)
            => base.VisitOrExpression(_c);
        public override object VisitParensExpression([NotNull] VooDoParser.ParensExpressionContext _c)
            => base.VisitParensExpression(_c);
        public override object VisitPredefinedClassType([NotNull] VooDoParser.PredefinedClassTypeContext _c)
            => base.VisitPredefinedClassType(_c);
        public override object VisitPredefinedStructType([NotNull] VooDoParser.PredefinedStructTypeContext _c)
            => base.VisitPredefinedStructType(_c);
        public override object VisitPredefinedType([NotNull] VooDoParser.PredefinedTypeContext _c)
            => base.VisitPredefinedType(_c);
        public override object VisitQualifiedAliasGenericName([NotNull] VooDoParser.QualifiedAliasGenericNameContext _c)
            => base.VisitQualifiedAliasGenericName(_c);
        public override object VisitReturnStatement([NotNull] VooDoParser.ReturnStatementContext _c)
            => base.VisitReturnStatement(_c);
        public override object VisitScript([NotNull] VooDoParser.ScriptContext _c)
            => base.VisitScript(_c);
        public override object VisitShiftExpressionGroup([NotNull] VooDoParser.ShiftExpressionGroupContext _c)
            => base.VisitShiftExpressionGroup(_c);
        public override object VisitSpecificCatchClause([NotNull] VooDoParser.SpecificCatchClauseContext _c)
            => base.VisitSpecificCatchClause(_c);
        public override object VisitThrowExpression([NotNull] VooDoParser.ThrowExpressionContext _c)
            => base.VisitThrowExpression(_c);
        public override object VisitThrowStatement([NotNull] VooDoParser.ThrowStatementContext _c)
            => base.VisitThrowStatement(_c);
        public override object VisitTryStatement([NotNull] VooDoParser.TryStatementContext _c)
            => base.VisitTryStatement(_c);
        public override object VisitTupleExpression([NotNull] VooDoParser.TupleExpressionContext _c)
            => base.VisitTupleExpression(_c);
        public override object VisitTupleType([NotNull] VooDoParser.TupleTypeContext _c)
            => base.VisitTupleType(_c);
        public override object VisitTupleTypeElement([NotNull] VooDoParser.TupleTypeElementContext _c)
            => base.VisitTupleTypeElement(_c);
        public override object VisitType([NotNull] VooDoParser.TypeContext _c)
            => base.VisitType(_c);
        public override object VisitTypeArgumentList([NotNull] VooDoParser.TypeArgumentListContext _c)
            => base.VisitTypeArgumentList(_c);
        public override object VisitTypeMemberAccessExpression([NotNull] VooDoParser.TypeMemberAccessExpressionContext _c)
            => base.VisitTypeMemberAccessExpression(_c);
        public override object VisitTypeMemberAccessExpressionSource([NotNull] VooDoParser.TypeMemberAccessExpressionSourceContext _c)
            => base.VisitTypeMemberAccessExpressionSource(_c);
        public override object VisitUnaryMinusExpression([NotNull] VooDoParser.UnaryMinusExpressionContext _c)
            => base.VisitUnaryMinusExpression(_c);
        public override object VisitUnaryPlusExpression([NotNull] VooDoParser.UnaryPlusExpressionContext _c)
            => base.VisitUnaryPlusExpression(_c);
        public override object VisitUsing([NotNull] VooDoParser.UsingContext _c)
            => base.VisitUsing(_c);
        public override object VisitUsingAlias([NotNull] VooDoParser.UsingAliasContext _c)
            => base.VisitUsingAlias(_c);
        public override object VisitVariableDeclaration([NotNull] VooDoParser.VariableDeclarationContext _c)
            => base.VisitVariableDeclaration(_c);
        public override object VisitVariableDeclarationStatement([NotNull] VooDoParser.VariableDeclarationStatementContext _c)
            => base.VisitVariableDeclarationStatement(_c);
        public override object VisitVariableDeclarator([NotNull] VooDoParser.VariableDeclaratorContext _c)
            => base.VisitVariableDeclarator(_c);
        public override object VisitXorExpression([NotNull] VooDoParser.XorExpressionContext _c)
            => base.VisitXorExpression(_c);

    }

}
