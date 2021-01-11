using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;


namespace VooDo.Transformation
{

    internal sealed class ExpressionHookGluer
    {

        private readonly List<IHookInitializer> m_hookInitializers;

        internal ExpressionSyntax HookSubscribeCallable { get; }

        internal IReadOnlyList<IHookInitializer> HookInitializers { get; }

        internal IHookInitializerProvider HookInitializerProvider { get; }
        private bool Glue(IHookInitializer _hookInitializer, out int _hookIndex)
        {
            if (_hookInitializer == null)
            {
                _hookIndex = -1;
                return false;
            }
            else
            {
                _hookIndex = m_hookInitializers.Count;
                m_hookInitializers.Add(_hookInitializer);
                return true;
            }
        }

        private bool TryGlue(MemberAccessExpressionSyntax _syntax, SemanticModel _model, out int _hookIndex)
            => Glue(HookInitializerProvider.GetHookInitializer(_syntax, _model), out _hookIndex);

        private bool TryGlue(ElementAccessExpressionSyntax _syntax, SemanticModel _model, out int _hookIndex)
            => Glue(HookInitializerProvider.GetHookInitializer(_syntax, _model), out _hookIndex);

        private ExpressionSyntax CreateHookSubscribeExpressionSyntax(ExpressionSyntax _source, int _hookIndex)
        {
            ArgumentSyntax sourceArgument = SyntaxFactory.Argument(_source);
            ArgumentSyntax hookIndexArgument = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(_hookIndex)));
            SyntaxNodeOrToken[] arguments = new SyntaxNodeOrToken[] { sourceArgument, SyntaxFactory.Token(SyntaxKind.CommaToken), hookIndexArgument };
            return SyntaxFactory.InvocationExpression(HookSubscribeCallable, SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(arguments)));
        }

        private sealed class ExpressionRewriter : CSharpSyntaxRewriter
        {

            private readonly ExpressionHookGluer m_gluer;
            private readonly SemanticModel m_semantics;

            internal ExpressionRewriter(ExpressionHookGluer _gluer, SemanticModel _semantics)
            {
                m_gluer = _gluer;
                m_semantics = _semantics;
            }

            public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax _node)
            {
                if (m_gluer.TryGlue(_node, m_semantics, out int hookIndex))
                {
                    return _node.WithExpression(m_gluer.CreateHookSubscribeExpressionSyntax((ExpressionSyntax) Visit(_node.Expression), hookIndex));
                }
                else
                {
                    return _node;
                }
            }

            public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax _node)
            {
                if (m_gluer.TryGlue(_node, m_semantics, out int hookIndex))
                {
                    return _node.WithExpression(m_gluer.CreateHookSubscribeExpressionSyntax((ExpressionSyntax) Visit(_node.Expression), hookIndex));
                }
                else
                {
                    return _node;
                }
            }

        }

        private sealed class RValueRewriter : CSharpSyntaxRewriter
        {

            private readonly ExpressionRewriter m_exprRewriter;

            internal RValueRewriter(ExpressionHookGluer _gluer, SemanticModel _semantics) => m_exprRewriter = new ExpressionRewriter(_gluer, _semantics);

            public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax _node) => _node.WithRight((ExpressionSyntax) m_exprRewriter.Visit(_node.Right));
            public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax _node) => _node.WithValue((ExpressionSyntax) m_exprRewriter.Visit(_node.Value));

        }

        public ExpressionHookGluer(IHookInitializerProvider _initializerProvider, ExpressionSyntax _subscribeCallable)
        {
            if (_initializerProvider == null)
            {
                throw new ArgumentNullException(nameof(_initializerProvider));
            }
            if (_subscribeCallable == null)
            {
                throw new ArgumentNullException(nameof(_subscribeCallable));
            }
            HookInitializerProvider = _initializerProvider;
            HookSubscribeCallable = _subscribeCallable;
            m_hookInitializers = new List<IHookInitializer>();
            HookInitializers = m_hookInitializers.AsReadOnly();
        }

        public SyntaxNode Glue(SemanticModel _semantics)
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            return Glue(_semantics, _semantics.SyntaxTree.GetRoot());
        }

        public SyntaxNode Glue(SemanticModel _semantics, SyntaxNode _root)
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            if (_root == null)
            {
                throw new ArgumentNullException(nameof(_root));
            }
            return new RValueRewriter(this, _semantics).Visit(_root);
        }
    }

}
