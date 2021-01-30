using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Transformation
{
    // TODO Make internal
    public static class GlobalVariableAccessRewriter
    {

        private static readonly string s_nameOfKeyword = SyntaxFactory.Token(SyntaxKind.NameOfKeyword).ValueText;

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
                => TryCreateValueOfSyntax(_node) ?? base.VisitIdentifierName(_node);

            private ExpressionSyntax TryCreateControllerOfSyntax(InvocationExpressionSyntax _node)
            {
                if (_node.ArgumentList.Arguments.Count == 1)
                {
                    ArgumentSyntax argument = _node.ArgumentList.Arguments[0];
                    if (argument.RefKindKeyword.IsKind(SyntaxKind.None))
                    {
                        string variableName = GetTargetSymbolName(argument.Expression);
                        if (variableName != null)
                        {
                            return CreateAccessSyntax(variableName, true);
                        }
                        else
                        {
                            throw new Exception("controllerof can be only applied to global variables"); //TODO Emit diagnostic
                        }
                    }
                    else
                    {
                        throw new Exception("controllerof does not take ref or out arguments"); //TODO Emit diagnostic
                    }
                }
                else
                {
                    throw new Exception("controllerof takes 1 argument"); //TODO Emit diagnostic
                }
            }

            private ExpressionSyntax TryCreateNameOfSyntax(InvocationExpressionSyntax _node)
            {
                if (_node.ArgumentList.Arguments.Count == 1)
                {
                    ArgumentSyntax argument = _node.ArgumentList.Arguments[0];
                    if (argument.RefKindKeyword.IsKind(SyntaxKind.None))
                    {
                        string variableName = GetTargetSymbolName(argument.Expression);
                        if (variableName != null)
                        {
                            return CreateStringLiteralSyntax(variableName);
                        }
                    }
                }
                return null;
            }

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax _node)
            {
                if (_node.Expression is IdentifierNameSyntax name)
                {
                    ExpressionSyntax expression = null;
                    if (name.Identifier.Text == Identifiers.controllerOfMacro)
                    {
                        expression = TryCreateControllerOfSyntax(_node);
                    }
                    else if (name.Identifier.Text == s_nameOfKeyword)
                    {
                        expression = TryCreateNameOfSyntax(_node);
                    }
                    if (expression != null)
                    {
                        return OriginalSpanRewriter.RewriteAbsolute(expression, _node.GetOriginalSpan());
                    }
                }
                return base.VisitInvocationExpression(_node);
            }

            private static ExpressionSyntax CreateStringLiteralSyntax(string _name) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(_name));

            private static ExpressionSyntax CreateAccessSyntax(string _name, bool _controller)
            {
                string accessorName = _controller ? nameof(Variable<object>.Controller) : nameof(Variable<object>.Value);
                MemberAccessExpressionSyntax globals = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName(Identifiers.globalsField));
                MemberAccessExpressionSyntax variable = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    globals,
                    SyntaxFactory.IdentifierName(string.Format(Identifiers.globalVariableFormat, _name)));
                MemberAccessExpressionSyntax accessor = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    variable,
                    SyntaxFactory.IdentifierName(accessorName));
                return accessor;
            }

            private ExpressionSyntax TryCreateValueOfSyntax(ExpressionSyntax _node)
            {
                string variableName = GetTargetSymbolName(_node);
                if (variableName != null)
                {
                    ExpressionSyntax access = CreateAccessSyntax(variableName, false);
                    return OriginalSpanRewriter.RewriteAbsolute(access, _node.GetOriginalOrFullSpan());
                }
                return null;
            }

        }

        public static TNode Rewrite<TNode>(TNode _syntax, SemanticModel _semantics, IEnumerable<VariableDeclaratorSyntax> _globalDeclarations) where TNode : SyntaxNode
        {
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            if (_globalDeclarations == null)
            {
                throw new ArgumentNullException(nameof(_globalDeclarations));
            }
            return Rewrite(_syntax, _semantics, _globalDeclarations.Select(_d => _semantics.GetDeclaredSymbol(_d)));
        }

        public static TNode Rewrite<TNode>(TNode _syntax, SemanticModel _semantics, IEnumerable<ISymbol> _globalSymbols) where TNode : SyntaxNode
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            if (_globalSymbols == null)
            {
                throw new ArgumentNullException(nameof(_globalSymbols));
            }
            HashSet<ISymbol> symbolSet = new HashSet<ISymbol>(_globalSymbols, SymbolEqualityComparer.Default);
            if (symbolSet.Contains(null))
            {
                throw new ArgumentException("Null symbol", nameof(_globalSymbols));
            }
            Rewriter rewriter = new Rewriter(_semantics, symbolSet);
            return (TNode) rewriter.Visit(_syntax);
        }

    }
}