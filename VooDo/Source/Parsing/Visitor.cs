
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST;
using VooDo.AST.Directives;
using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Parsing.Generated;

namespace VooDo.Parsing
{

    internal sealed class Visitor : VooDoParserBaseVisitor<NodeOrIdentifier>
    {

        private readonly string m_source;

        internal Visitor(string _source)
        {
            m_source = _source;
        }

        private Origin MakeOrigin(int _start, int _end)
            => new CodeOrigin(_start, _end - _start, m_source);

        private Origin GetOrigin(IToken _token)
            => MakeOrigin(_token.StartIndex, _token.StopIndex);

        private Origin GetOrigin(ParserRuleContext _context)
            => MakeOrigin(_context.Start.StartIndex, (_context.Stop ?? _context.Start).StopIndex);

        private ImmutableArray<TNodeOrIdentifier> Get<TNodeOrIdentifier>(IEnumerable<ParserRuleContext> _rule) where TNodeOrIdentifier : NodeOrIdentifier
            => _rule.Select(Get<TNodeOrIdentifier>).ToImmutableArray();

        private TNodeOrIdentifier Variant<TNodeOrIdentifier>(ParserRuleContext _rule) where TNodeOrIdentifier : NodeOrIdentifier
            => Get<TNodeOrIdentifier>(_rule.GetRuleContexts<ParserRuleContext>().Single(_r => _r is not null));

        private TNodeOrIdentifier Get<TNodeOrIdentifier>(ParserRuleContext _rule) where TNodeOrIdentifier : NodeOrIdentifier
            => TryGet<TNodeOrIdentifier>(_rule)!;

        private TNodeOrIdentifier? TryGet<TNodeOrIdentifier>(ParserRuleContext _rule) where TNodeOrIdentifier : NodeOrIdentifier
            => _rule is null ? null : (TNodeOrIdentifier?) Visit(_rule);

        public override NodeOrIdentifier Visit(IParseTree _tree)
        {
            NodeOrIdentifier result = base.Visit(_tree);
            if (_tree is ParserRuleContext context)
            {
                result = result with
                {
                    Origin = GetOrigin(context)
                };
            }
            else if (_tree is IToken token)
            {
                result = result with
                {
                    Origin = GetOrigin(token)
                };
            }
            return result;
        }

        public override NodeOrIdentifier VisitArgument([NotNull] VooDoParser.ArgumentContext _c)
            => Get<InvocationExpression.Argument>(_c.mArgument) with { Parameter = TryGet<Identifier>(_c.mParam) };
        public override NodeOrIdentifier VisitArrayCreationExpression([NotNull] VooDoParser.ArrayCreationExpressionContext _c)
        {
            ImmutableArray<Expression> sizes = Get<Expression>(_c._mSizes);
            ImmutableArray<ComplexType.RankSpecifier> additionalRanks = Get<ComplexType.RankSpecifier>(_c._mRanks);
            ComplexType.RankSpecifier firstRank = new ComplexType.RankSpecifier(sizes.Length) with { Origin = GetOrigin(_c) };
            ComplexType type = Get<ComplexType>(_c.mType) with
            {
                Ranks = additionalRanks.Insert(0, firstRank)
            };
            return new ArrayCreationExpression(type, sizes);
        }
        public override NodeOrIdentifier VisitAsExpression([NotNull] VooDoParser.AsExpressionContext _c)
            => new AsExpression(Get<Expression>(_c.mExpr), Get<ComplexType>(_c.mType));
        public override NodeOrIdentifier VisitAssignableArgument([NotNull] VooDoParser.AssignableArgumentContext _c)
        {
            InvocationExpression.Argument.EKind kind = _c.mKind?.Type switch
            {
                VooDoParser.IN => InvocationExpression.Argument.EKind.In,
                VooDoParser.OUT => InvocationExpression.Argument.EKind.Out,
                VooDoParser.REF => InvocationExpression.Argument.EKind.Ref,
                null => InvocationExpression.Argument.EKind.Value,
                _ => throw new FormatException("Unexpected argument kind")
            };
            return new InvocationExpression.AssignableArgument(null, kind, Get<AssignableExpression>(_c.mValue));
        }
        public override NodeOrIdentifier VisitAssignableExpression([NotNull] VooDoParser.AssignableExpressionContext _c)
            => Variant<AssignableExpression>(_c);
        public override NodeOrIdentifier VisitAssignmentStatement([NotNull] VooDoParser.AssignmentStatementContext _c)
        {
            AssignmentStatement.EKind kind = _c.mOp.Type switch
            {
                VooDoParser.ASSIGN => AssignmentStatement.EKind.Simple,
                VooDoParser.ASSIGN_ADD => AssignmentStatement.EKind.Add,
                VooDoParser.ASSIGN_AND => AssignmentStatement.EKind.BitwiseAnd,
                VooDoParser.ASSIGN_COAL => AssignmentStatement.EKind.Coalesce,
                VooDoParser.ASSIGN_DIV => AssignmentStatement.EKind.Divide,
                VooDoParser.ASSIGN_LSH => AssignmentStatement.EKind.LeftShift,
                VooDoParser.ASSIGN_MOD => AssignmentStatement.EKind.Modulo,
                VooDoParser.ASSIGN_MUL => AssignmentStatement.EKind.Multiply,
                VooDoParser.ASSIGN_OR => AssignmentStatement.EKind.BitwiseOr,
                VooDoParser.ASSIGN_RSH => AssignmentStatement.EKind.RightShift,
                VooDoParser.ASSIGN_SUB => AssignmentStatement.EKind.Subtract,
                VooDoParser.ASSIGN_XOR => AssignmentStatement.EKind.BitwiseXor,
                _ => throw new FormatException("Unexpected operation kind")

            };
            return new AssignmentStatement(Get<AssignableExpression>(_c.mTarget), kind, Get<Expression>(_c.mSource));
        }
        public override NodeOrIdentifier VisitBinaryExpression([NotNull] VooDoParser.BinaryExpressionContext _c)
        {
            BinaryExpression.EKind kind = _c.mOp.Type switch
            {
                VooDoParser.PLUS => BinaryExpression.EKind.Add,
                VooDoParser.MINUS => BinaryExpression.EKind.Subtract,
                VooDoParser.MUL => BinaryExpression.EKind.Multiply,
                VooDoParser.DIV => BinaryExpression.EKind.Divide,
                VooDoParser.MOD => BinaryExpression.EKind.Modulo,
                VooDoParser.LSH => BinaryExpression.EKind.LeftShift,
                VooDoParser.RSH => BinaryExpression.EKind.RightShift,
                VooDoParser.AND => BinaryExpression.EKind.BitwiseAnd,
                VooDoParser.OR => BinaryExpression.EKind.BitwiseOr,
                VooDoParser.XOR => BinaryExpression.EKind.BitwiseXor,
                VooDoParser.LAND => BinaryExpression.EKind.LogicAnd,
                VooDoParser.LOR => BinaryExpression.EKind.LogicOr,
                VooDoParser.COAL => BinaryExpression.EKind.Coalesce,
                VooDoParser.GT => BinaryExpression.EKind.GreaterThan,
                VooDoParser.LT => BinaryExpression.EKind.LessThan,
                VooDoParser.GE => BinaryExpression.EKind.GreaterThanOrEqual,
                VooDoParser.LE => BinaryExpression.EKind.LessThanOrEqual,
                VooDoParser.EQ => BinaryExpression.EKind.Equals,
                VooDoParser.NEQ => BinaryExpression.EKind.NotEquals,
                _ => throw new FormatException("Unexpected operation kind")
            };
            return new BinaryExpression(Get<Expression>(_c.mLeft), kind, Get<Expression>(_c.mRight));
        }
        public override NodeOrIdentifier VisitBlockStatement([NotNull] VooDoParser.BlockStatementContext _c)
            => new BlockStatement(Get<Statement>(_c._mStatements));
        public override NodeOrIdentifier VisitCastExpression([NotNull] VooDoParser.CastExpressionContext _c)
            => new CastExpression(Get<ComplexType>(_c.mType), Get<Expression>(_c.mExpr));
        public override NodeOrIdentifier VisitComplexType([NotNull] VooDoParser.ComplexTypeContext _c)
            => Variant<ComplexType>(_c);
        public override NodeOrIdentifier VisitComplexTypeBase([NotNull] VooDoParser.ComplexTypeBaseContext _c)
            => Variant<ComplexType>(_c);
        public override NodeOrIdentifier VisitComplexTypeOrExpression([NotNull] VooDoParser.ComplexTypeOrExpressionContext _c)
            => Variant<ComplexTypeOrExpression>(_c);
        public override NodeOrIdentifier VisitComplexTypeOrVar([NotNull] VooDoParser.ComplexTypeOrVarContext _c)
            => _c.mType is null ? ComplexTypeOrVar.Var : ComplexTypeOrVar.FromComplexType(Get<ComplexType>(_c.mType));
        public override NodeOrIdentifier VisitConditionalExpression([NotNull] VooDoParser.ConditionalExpressionContext _c)
            => new ConditionalExpression(Get<Expression>(_c.mCond), Get<Expression>(_c.mTrue), Get<Expression>(_c.mFalse));
        public override NodeOrIdentifier VisitDeclarationStatement([NotNull] VooDoParser.DeclarationStatementContext _c)
            => new DeclarationStatement(Get<ComplexTypeOrVar>(_c.mType), Get<DeclarationStatement.Declarator>(_c._mDeclarators));
        public override NodeOrIdentifier VisitDeclarator([NotNull] VooDoParser.DeclaratorContext _c)
            => new DeclarationStatement.Declarator(Get<Identifier>(_c.mName), TryGet<Expression>(_c.mInitializer));
        public override NodeOrIdentifier VisitDefaultExpression([NotNull] VooDoParser.DefaultExpressionContext _c)
            => new DefaultExpression(TryGet<ComplexType>(_c.mType));
        public override NodeOrIdentifier VisitElementAccessExpression([NotNull] VooDoParser.ElementAccessExpressionContext _c)
            => new ElementAccessExpression(Get<Expression>(_c.mSource), Get<Expression>(_c._mArgs));
        public override NodeOrIdentifier VisitExpressionStatement([NotNull] VooDoParser.ExpressionStatementContext _c)
            => new ExpressionStatement(Get<InvocationOrObjectCreationExpression>(_c.mExpr));
        public override NodeOrIdentifier VisitFalseLiteralExpression([NotNull] VooDoParser.FalseLiteralExpressionContext _c)
            => LiteralExpression.False;
        public override NodeOrIdentifier VisitFullScript([NotNull] VooDoParser.FullScriptContext _c)
            => new Script(Get<UsingDirective>(_c._mUsings), Get<Statement>(_c._mBody));
        public override NodeOrIdentifier VisitGlobalExpression([NotNull] VooDoParser.GlobalExpressionContext _c)
            => new GlobalExpression(Get<Expression>(_c.mController), TryGet<Expression>(_c.mInitializer));
        public override NodeOrIdentifier VisitGlobalStatement([NotNull] VooDoParser.GlobalStatementContext _c)
            => new GlobalStatement(Get<DeclarationStatement>(_c._mDeclarations));
        public override NodeOrIdentifier VisitIdentifier([NotNull] VooDoParser.IdentifierContext _c)
            => new Identifier(_c.GetText());
        public override NodeOrIdentifier VisitIdentifierOrDiscard([NotNull] VooDoParser.IdentifierOrDiscardContext _c)
            => _c.mIdentifier is null ? IdentifierOrDiscard.Discard : Get<Identifier>(_c.mIdentifier);
        public override NodeOrIdentifier VisitIfStatement([NotNull] VooDoParser.IfStatementContext _c)
            => new IfStatement(Get<Expression>(_c.mCondition), Get<Statement>(_c.mThen), TryGet<Statement>(_c.mElse));
        public override NodeOrIdentifier VisitInlineScript([NotNull] VooDoParser.InlineScriptContext _c)
        {
            Origin origin = GetOrigin(_c);
            ReturnStatement expression = new ReturnStatement(Get<Expression>(_c.mExpr)) with { Origin = origin };
            return new Script(default, ImmutableArray.Create<Statement>(expression));
        }
        public override NodeOrIdentifier VisitIsExpression([NotNull] VooDoParser.IsExpressionContext _c)
            => new IsExpression(Get<Expression>(_c.mSource), Get<ComplexType>(_c.mType), TryGet<IdentifierOrDiscard>(_c.mName));
        public override NodeOrIdentifier VisitMemberAccessExpression([NotNull] VooDoParser.MemberAccessExpressionContext _c)
            => new MemberAccessExpression(Get<ComplexTypeOrExpression>(_c.mSource), Get<Identifier>(_c.mMember));
        public override NodeOrIdentifier VisitMethod([NotNull] VooDoParser.MethodContext _c)
            => new InvocationExpression.Method(Get<NameOrMemberAccessExpression>(_c.mSource), Get<ComplexType>(_c._mTypeArgs));
        public override NodeOrIdentifier VisitMethodInvocationExpression([NotNull] VooDoParser.MethodInvocationExpressionContext _c)
            => new InvocationExpression(Get<InvocationExpression.Method>(_c.mSource), Get<InvocationExpression.Argument>(_c._mArgs));
        public override NodeOrIdentifier VisitNameExpression([NotNull] VooDoParser.NameExpressionContext _c)
            => new NameExpression(_c.mControllerOf is not null, Get<Identifier>(_c.mName));
        public override NodeOrIdentifier VisitNameOrMemberAccessExpression([NotNull] VooDoParser.NameOrMemberAccessExpressionContext _c)
            => Variant<NameOrMemberAccessExpression>(_c);
        public override NodeOrIdentifier VisitNamespace([NotNull] VooDoParser.NamespaceContext _c)
            => new Namespace(TryGet<Identifier>(_c.mAlias), Get<Identifier>(_c._mPath));
        public override NodeOrIdentifier VisitNullLiteralExpression([NotNull] VooDoParser.NullLiteralExpressionContext _c)
            => LiteralExpression.Null;
        public override NodeOrIdentifier VisitObjectCreationExpression([NotNull] VooDoParser.ObjectCreationExpressionContext _c)
            => new ObjectCreationExpression(TryGet<ComplexType>(_c.mType), Get<InvocationExpression.Argument>(_c._mArgs));
        public override NodeOrIdentifier VisitOtherExpression([NotNull] VooDoParser.OtherExpressionContext _c)
            => Variant<Expression>(_c);
        public override NodeOrIdentifier VisitOtherLiteralExpression([NotNull] VooDoParser.OtherLiteralExpressionContext _c)
            => new LiteralExpression(((LiteralExpressionSyntax) SyntaxFactory.ParseExpression(_c.GetText())).Token.Value);
        public override NodeOrIdentifier VisitOtherStatement([NotNull] VooDoParser.OtherStatementContext _c)
            => Variant<Statement>(_c);
        public override NodeOrIdentifier VisitOutDeclarationArgumentWithDiscard([NotNull] VooDoParser.OutDeclarationArgumentWithDiscardContext _c)
            => new InvocationExpression.OutDeclarationArgument(null, ComplexTypeOrVar.Var, IdentifierOrDiscard.Discard);
        public override NodeOrIdentifier VisitOutDeclarationArgumentWithType([NotNull] VooDoParser.OutDeclarationArgumentWithTypeContext _c)
            => new InvocationExpression.OutDeclarationArgument(null, Get<ComplexTypeOrVar>(_c.mType), Get<IdentifierOrDiscard>(_c.mName));
        public override NodeOrIdentifier VisitParenthesizedExpression([NotNull] VooDoParser.ParenthesizedExpressionContext _c)
            => Get<Expression>(_c.mExpr);
        public override NodeOrIdentifier VisitQualifiedType([NotNull] VooDoParser.QualifiedTypeContext _c)
            => Get<QualifiedType>(_c.mBase) with { IsNullable = _c.mNullable is not null, Ranks = Get<ComplexType.RankSpecifier>(_c._mRanks) };
        public override NodeOrIdentifier VisitQualifiedTypeBase([NotNull] VooDoParser.QualifiedTypeBaseContext _c)
            => new QualifiedType(TryGet<Identifier>(_c.mAlias), Get<SimpleType>(_c._mPath));
        public override NodeOrIdentifier VisitRankSpecifier([NotNull] VooDoParser.RankSpecifierContext _c)
            => new ComplexType.RankSpecifier(_c._mCommas.Count + 1);
        public override NodeOrIdentifier VisitReturnStatement([NotNull] VooDoParser.ReturnStatementContext _c)
            => new ReturnStatement(Get<Expression>(_c.mExpr));
        public override NodeOrIdentifier VisitSimpleInvocationExpression([NotNull] VooDoParser.SimpleInvocationExpressionContext _c)
            => new InvocationExpression(Get<InvocationExpression.SimpleCallable>(_c.mSource), Get<InvocationExpression.Argument>(_c._mArgs));
        public override NodeOrIdentifier VisitSimpleType([NotNull] VooDoParser.SimpleTypeContext _c)
            => new SimpleType(Get<Identifier>(_c.mName), Get<ComplexType>(_c._mTypeArgs));
        public override NodeOrIdentifier VisitTrueLiteralExpression([NotNull] VooDoParser.TrueLiteralExpressionContext _c)
            => LiteralExpression.True;
        public override NodeOrIdentifier VisitTupleDeclarationExpression([NotNull] VooDoParser.TupleDeclarationExpressionContext _c)
            => new TupleDeclarationExpression(Get<TupleDeclarationExpression.Element>(_c._mElements));
        public override NodeOrIdentifier VisitTupleDeclarationExpressionElement([NotNull] VooDoParser.TupleDeclarationExpressionElementContext _c)
            => new TupleDeclarationExpression.Element(Get<ComplexTypeOrVar>(_c.mType), Get<IdentifierOrDiscard>(_c.mName));
        public override NodeOrIdentifier VisitTupleExpression([NotNull] VooDoParser.TupleExpressionContext _c)
            => new TupleExpression(Get<TupleExpression.Element>(_c._mElements));
        public override NodeOrIdentifier VisitTupleExpressionElement([NotNull] VooDoParser.TupleExpressionElementContext _c)
            => new TupleExpression.Element(TryGet<Identifier>(_c.mName), Get<Expression>(_c.mExpr));
        public override NodeOrIdentifier VisitTupleType([NotNull] VooDoParser.TupleTypeContext _c)
            => Get<QualifiedType>(_c.mBase) with { IsNullable = _c.mNullable is not null, Ranks = Get<ComplexType.RankSpecifier>(_c._mRanks) };
        public override NodeOrIdentifier VisitTupleTypeBase([NotNull] VooDoParser.TupleTypeBaseContext _c)
            => new TupleType(Get<TupleType.Element>(_c._mElements));
        public override NodeOrIdentifier VisitTupleTypeElement([NotNull] VooDoParser.TupleTypeElementContext _c)
            => new TupleType.Element(Get<ComplexType>(_c.mType), TryGet<Identifier>(_c.mName));
        public override NodeOrIdentifier VisitUnaryExpression([NotNull] VooDoParser.UnaryExpressionContext _c)
        {
            UnaryExpression.EKind kind = _c.mOp.Type switch
            {
                VooDoParser.PLUS => UnaryExpression.EKind.Plus,
                VooDoParser.MINUS => UnaryExpression.EKind.Minus,
                VooDoParser.NOT => UnaryExpression.EKind.LogicNot,
                VooDoParser.BNOT => UnaryExpression.EKind.BitwiseNot,
                _ => throw new FormatException("Unexpected operation kind")
            };
            return new UnaryExpression(kind, Get<Expression>(_c.mExpr));
        }
        public override NodeOrIdentifier VisitUnnamedArgument([NotNull] VooDoParser.UnnamedArgumentContext _c)
            => Variant<InvocationExpression.Argument>(_c);
        public override NodeOrIdentifier VisitUsingNamespaceDirective([NotNull] VooDoParser.UsingNamespaceDirectiveContext _c)
            => new UsingNamespaceDirective(TryGet<Identifier>(_c.mAlias), Get<Namespace>(_c.mName));
        public override NodeOrIdentifier VisitUsingStaticDirective([NotNull] VooDoParser.UsingStaticDirectiveContext _c)
            => new UsingStaticDirective(Get<QualifiedType>(_c.mType));
        public override NodeOrIdentifier VisitValueArgument([NotNull] VooDoParser.ValueArgumentContext _c)
            => new InvocationExpression.ValueArgument(null, Get<Expression>(_c.mValue));

        public override NodeOrIdentifier VisitScript_Greedy([NotNull] VooDoParser.Script_GreedyContext _c) => Variant<Script>(_c);
        public override NodeOrIdentifier VisitUsingDirective_Greedy([NotNull] VooDoParser.UsingDirective_GreedyContext _c) => Variant<UsingDirective>(_c);
        public override NodeOrIdentifier VisitStatement_Greedy([NotNull] VooDoParser.Statement_GreedyContext _c) => Variant<Statement>(_c);
        public override NodeOrIdentifier VisitExpression_Greedy([NotNull] VooDoParser.Expression_GreedyContext _c) => Variant<Expression>(_c);
        public override NodeOrIdentifier VisitIdentifier_Greedy([NotNull] VooDoParser.Identifier_GreedyContext _c) => Variant<Identifier>(_c);
        public override NodeOrIdentifier VisitIdentifierOrDiscard_Greedy([NotNull] VooDoParser.IdentifierOrDiscard_GreedyContext _c) => Variant<IdentifierOrDiscard>(_c);
        public override NodeOrIdentifier VisitNamespace_Greedy([NotNull] VooDoParser.Namespace_GreedyContext _c) => Variant<Namespace>(_c);
        public override NodeOrIdentifier VisitSimpleType_Greedy([NotNull] VooDoParser.SimpleType_GreedyContext _c) => Variant<SimpleType>(_c);
        public override NodeOrIdentifier VisitComplexType_Greedy([NotNull] VooDoParser.ComplexType_GreedyContext _c) => Variant<ComplexType>(_c);
        public override NodeOrIdentifier VisitComplexTypeOrVar_Greedy([NotNull] VooDoParser.ComplexTypeOrVar_GreedyContext _c) => Variant<ComplexTypeOrVar>(_c);
        public override NodeOrIdentifier VisitQualifiedType_Greedy([NotNull] VooDoParser.QualifiedType_GreedyContext _c) => Variant<QualifiedType>(_c);
        public override NodeOrIdentifier VisitTupleType_Greedy([NotNull] VooDoParser.TupleType_GreedyContext _c) => Variant<TupleType>(_c);
        public override NodeOrIdentifier VisitComplexTypeOrExpression_Greedy([NotNull] VooDoParser.ComplexTypeOrExpression_GreedyContext _c) => Variant<ComplexTypeOrExpression>(_c);

    }

}
