using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Transformation
{
    // TODO Make internal
    public static class GlobalVariableAccessTransformer
    {

        public const string controllerOfMacro = "VooDo_internal_controllerof";
        public const string globalsClassName = "VooDo_internal_Globals";
        public const string variableNameFormat = "VooDo_internal_field_{0}";

        private sealed class Rewriter : CSharpSyntaxRewriter
        {

            private readonly IReadOnlyCollection<ISymbol> m_symbols;
            private readonly SemanticModel m_semantics;

            internal Rewriter(SemanticModel _semantics, IReadOnlyCollection<ISymbol> _symbols)
            {
                m_symbols = _symbols;
                m_semantics = _semantics;
            }

            private string GetTargetSymbolName(ExpressionSyntax _node)
            {
                SymbolInfo symbol = m_semantics.GetSymbolInfo(_node);
                if (symbol.Symbol != null && m_symbols.Contains(symbol.Symbol))
                {
                    return symbol.Symbol.Name;
                }
                else
                {
                    return null;
                }
            }

            public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax _node)
                => _node
                .WithArgumentList((BracketedArgumentListSyntax) Visit(_node.ArgumentList))
                .WithInitializer((EqualsValueClauseSyntax) Visit(_node.Initializer));

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax _node)
                => ProcessValueAccessSyntax((ExpressionSyntax) base.VisitIdentifierName(_node));

            public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax _node)
                => ProcessValueAccessSyntax((ExpressionSyntax) base.VisitMemberAccessExpression(_node));

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax _node)
            {
                if (_node.Expression is IdentifierNameSyntax name)
                {
                    if (_node.ArgumentList.Arguments.Count == 1)
                    {
                        ArgumentSyntax argument = _node.ArgumentList.Arguments[0];
                        if (argument.RefKindKeyword.IsKind(SyntaxKind.None))
                        {
                            string variableName = GetTargetSymbolName(argument.Expression);
                            if (variableName != null)
                            {
                                ExpressionSyntax node = null;
                                if (name.Identifier.ValueText == controllerOfMacro)
                                {
                                    node = CreateAccessSyntax(variableName, true);
                                }
                                else if (name.Identifier.ValueText == SyntaxFactory.Token(SyntaxKind.NameOfKeyword).ValueText)
                                {
                                    node = CreateStringLiteral(variableName);
                                }
                                if (node != null)
                                {
                                    return SpanTransformer.SetDescendantNodesSpan(node, _node.GetSpan());
                                }
                            }
                        }
                    }
                }
                return base.VisitInvocationExpression(_node);
            }

            private static ExpressionSyntax CreateStringLiteral(string _name) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(_name));

            private static ExpressionSyntax CreateAccessSyntax(string _name, bool _controller)
            {
                string accessorName = _controller ? nameof(Variable<object>.Controller) : nameof(Variable<object>.Value);
                MemberAccessExpressionSyntax globals = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName(globalsClassName));
                MemberAccessExpressionSyntax variable = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    globals,
                    SyntaxFactory.IdentifierName(string.Format(variableNameFormat, _name)));
                MemberAccessExpressionSyntax accessor = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    variable,
                    SyntaxFactory.IdentifierName(accessorName));
                return accessor;
            }

            private ExpressionSyntax ProcessValueAccessSyntax(ExpressionSyntax _node)
            {
                string variableName = GetTargetSymbolName(_node);
                if (variableName != null)
                {
                    ExpressionSyntax access = CreateAccessSyntax(variableName, false);
                    return SpanTransformer.SetDescendantNodesSpan(access, _node.GetSpan());
                }
                return _node;
            }

        }

        public static TNode ReplaceDescendantNodesAccess<TNode>(TNode _syntax, SemanticModel _semantics, IEnumerable<SyntaxNode> _symbolDeclarations) where TNode : SyntaxNode
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            if (_symbolDeclarations == null)
            {
                throw new ArgumentNullException(nameof(_symbolDeclarations));
            }
            return ReplaceDescendantNodesAccess(_syntax, _semantics, _symbolDeclarations.Select(_d => _semantics.GetDeclaredSymbol(_d)));
        }

        public static TNode ReplaceDescendantNodesAccess<TNode>(TNode _syntax, SemanticModel _semantics, IEnumerable<ISymbol> _symbols) where TNode : SyntaxNode
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            if (_symbols == null)
            {
                throw new ArgumentNullException(nameof(_symbols));
            }
            HashSet<ISymbol> symbolSet = new HashSet<ISymbol>(_symbols, SymbolEqualityComparer.Default);
            if (symbolSet.Contains(null))
            {
                throw new ArgumentException("Null symbol", nameof(_symbols));
            }
            Rewriter rewriter = new Rewriter(_semantics, symbolSet);
            return (TNode) rewriter.Visit(_syntax);
        }

    }
}