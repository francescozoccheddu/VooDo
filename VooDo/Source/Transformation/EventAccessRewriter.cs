using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.Transformation
{
    // TODO Set original spans correctly (keep original span for event arguments and source and replace for others)
    public static class EventAccessRewriter
    {

        private sealed class Rewriter : CSharpSyntaxRewriter
        {

            private readonly Dictionary<IEventSymbol, int> m_symbols = new Dictionary<IEventSymbol, int>(SymbolEqualityComparer.Default);
            private readonly HashSet<GetEventOverload> m_overloads = new HashSet<GetEventOverload>();
            private readonly SemanticModel m_semantics;

            internal ImmutableArray<IEventSymbol> Symbols => m_symbols.OrderBy(_e => _e.Value).Select(_e => _e.Key).ToImmutableArray();
            internal ImmutableHashSet<GetEventOverload> Overloads => m_overloads.ToImmutableHashSet();

            private IEventSymbol GetEventSymbol(ExpressionSyntax _syntax)
            {
                SymbolInfo symbolInfo = m_semantics.GetSymbolInfo(_syntax);
                if (symbolInfo.Symbol is IEventSymbol symbol)
                {
                    return symbol;
                }
                return null;
            }

            internal Rewriter(SemanticModel _semantics) => m_semantics = _semantics;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax _node)
                => TryCreateEventAccess(_node.Expression, _node.ArgumentList.Arguments) ?? base.VisitInvocationExpression(_node);

            public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax _node)
                => TryCreateEventAccess(_node, new ArgumentSyntax[0]) ?? base.VisitMemberAccessExpression(_node);

            private static GetEventOverload.EArgumentType GetArgumentType(ArgumentSyntax _argument)
            {
                if (_argument.RefKindKeyword.IsKind(SyntaxKind.RefKeyword))
                {
                    return GetEventOverload.EArgumentType.Ref;
                }
                else if (_argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword))
                {
                    return GetEventOverload.EArgumentType.Out;
                }
                else
                {
                    throw new Exception("Event catcher arguments must be out or ref"); //TODO Emit diagnostic
                }
            }

            private int AddSymbol(IEventSymbol _symbol)
            {
                if (!m_symbols.TryGetValue(_symbol, out int key))
                {
                    key = m_symbols.Count;
                    m_symbols.Add(_symbol, key);
                }
                return key;
            }

            private ExpressionSyntax TryCreateEventAccess(ExpressionSyntax _access, IReadOnlyList<ArgumentSyntax> _arguments)
            {
                IEventSymbol symbol = GetEventSymbol(_access);
                if (symbol != null)
                {
                    MemberAccessExpressionSyntax memberAccess = _access.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
                    if (memberAccess == null)
                    {
                        throw new Exception("Event access without member access syntax"); // TODO emit diagnostic
                    }
                    GetEventOverload overload = new GetEventOverload(_arguments.Select(GetArgumentType));
                    int key = AddSymbol(symbol);
                    m_overloads.Add(overload);
                    ExpressionSyntax[] argumentExpressions = _arguments.Select(_a => _a.Expression).ToArray(); // TODO Replace var declarations
                    // TODO Check for type correctness
                    return CreateEventAccess(memberAccess.Expression, overload, key, argumentExpressions);
                }
                return null;
            }

            private static ExpressionSyntax CreateEventAccess(ExpressionSyntax _source, GetEventOverload _overload, int _symbolIndex, IReadOnlyList<ExpressionSyntax> _arguments)
            {
                MemberAccessExpressionSyntax method = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName(_overload.Identifier));
                ArgumentSyntax symbolArgument = SyntaxFactory.Argument(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        SyntaxFactory.Literal(_symbolIndex)));
                ArgumentSyntax sourceArgument = SyntaxFactory.Argument(_source);
                ArgumentSyntax[] eventArguments = new ArgumentSyntax[_arguments.Count];
                for (int i = 0; i < _arguments.Count; i++)
                {
                    GetEventOverload.EArgumentType argumentType = _overload.Arguments[i];
                    SyntaxKind refOrOutKind;
                    switch (argumentType)
                    {
                        case GetEventOverload.EArgumentType.Ref:
                        refOrOutKind = SyntaxKind.RefKeyword;
                        break;
                        case GetEventOverload.EArgumentType.Out:
                        refOrOutKind = SyntaxKind.OutKeyword;
                        break;
                        default:
                        throw new Exception("Unexpected argument type");
                    }
                    eventArguments[i] = SyntaxFactory.Argument(_arguments[i])
                        .WithRefOrOutKeyword(SyntaxFactory.Token(refOrOutKind));
                }
                IEnumerable<ArgumentSyntax> arguments = new ArgumentSyntax[] { symbolArgument, sourceArgument }.Concat(eventArguments);
                InvocationExpressionSyntax invocation = SyntaxFactory.InvocationExpression(method)
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments)));
                return invocation;
            }

        }

        public static TNode Rewrite<TNode>(TNode _node, SemanticModel _semantics, out ImmutableArray<IEventSymbol> _symbols, out ImmutableHashSet<GetEventOverload> _overloads) where TNode : SyntaxNode
        {
            if (_node == null)
            {
                throw new ArgumentNullException(nameof(_node));
            }
            if (_semantics == null)
            {
                throw new ArgumentNullException(nameof(_semantics));
            }
            Rewriter rewriter = new Rewriter(_semantics);
            TNode newNode = (TNode) rewriter.Visit(_node);
            _symbols = rewriter.Symbols;
            _overloads = rewriter.Overloads;
            return newNode;
        }

    }

}
