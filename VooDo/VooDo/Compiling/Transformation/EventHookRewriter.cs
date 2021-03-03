using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.Compiling.Transformation
{

    internal static class EventHookRewriter
    {

        private sealed class Rewriter : CSharpSyntaxRewriter
        {

            private sealed class Entry
            {

                public Entry(int _index)
                {
                    Index = _index;
                }

                internal int Index { get; }
                internal int Count { get; set; }

            }

            public Rewriter(SemanticModel _semantics)
            {
                m_semantics = _semantics;
                m_map = new();
            }

            private readonly SemanticModel m_semantics;
            private readonly Dictionary<IEventSymbol, Entry> m_map;

            internal ImmutableArray<(IEventSymbol symbol, int count)> EventSets
                => m_map.OrderBy(_e => _e.Value.Index).Select(_e => (_e.Key, _e.Value.Count)).ToImmutableArray();

            public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax _node)
            {
                if (m_semantics.GetOperation(_node) is IEventReferenceOperation operation)
                {
                    if (operation.Instance!.Type!.IsReferenceType)
                    {
                        IEventSymbol symbol = operation.Event;
                        if (!m_map.TryGetValue(symbol, out Entry entry))
                        {
                            entry = new Entry(m_map.Count);
                            m_map[symbol] = entry;
                        }
                        return SyntaxFactoryUtils.Invocation(
                            SyntaxFactoryUtils.ThisMemberAccess(
                                Identifiers.subscribeEventMethodName),
                            (ExpressionSyntax)Visit(_node.Expression),
                            SyntaxFactoryUtils.Literal(entry.Index),
                            SyntaxFactoryUtils.Literal(entry.Count++))
                            .Own(_node.GetTag()!);
                    }
                }
                return base.VisitMemberAccessExpression(_node);
            }

        }

        private static ClassDeclarationSyntax CreateEventHook(IEventSymbol _symbol, int _uniqueIndex)
        {
            string objectName = "_object";
            string subscribedName = "_subscribed";
            ExpressionSyntax left = SyntaxFactoryUtils.MemberAccess(
                    SyntaxFactory.ParenthesizedExpression(
                        SyntaxFactory.CastExpression(
                            _symbol.ContainingType.ToTypeSyntax(),
                            SyntaxFactory.IdentifierName(objectName))),
                    _symbol.Name);
            ExpressionSyntax right = SyntaxFactoryUtils.ThisMemberAccess(
                    Identifiers.eventHookOnEventMethodName);
            return SyntaxFactory.ClassDeclaration(
               $"{Identifiers.eventHookClassPrefix}{_uniqueIndex}")
               .WithModifiers(
                   SyntaxFactoryUtils.Tokens(
                       SyntaxKind.PrivateKeyword,
                       SyntaxKind.SealedKeyword))
               .WithBaseList(
                   SyntaxFactory.BaseList(
                       SyntaxFactory.SimpleBaseType(
                           SyntaxFactory.IdentifierName(
                               Identifiers.eventHookClassName))
                       .ToSeparatedList<BaseTypeSyntax>()))
               .AddMembers(
                   SyntaxFactoryUtils.MethodDeclaration(
                       Identifiers.eventHookOnEventMethodName,
                       SyntaxFactory.ExpressionStatement(
                           SyntaxFactoryUtils.Invocation(
                               SyntaxFactoryUtils.BaseMemberAccess(
                                   Identifiers.eventHookNotifyMethodName))),
                       null,
                       ((INamedTypeSymbol)_symbol.Type)
                           .DelegateInvokeMethod!
                           .Parameters
                           .SelectIndexed((_p, _i) => (_p.Type.ToTypeSyntax(), $"_par{_i}"))),
                   SyntaxFactoryUtils.MethodOverride(
                       Identifiers.eventHookSetSubscribedMethodName,
                       SyntaxFactoryUtils.IfElse(
                           SyntaxFactory.IdentifierName(subscribedName),
                           SyntaxFactory.ExpressionStatement(
                               SyntaxFactory.AssignmentExpression(
                                   SyntaxKind.AddAssignmentExpression,
                                   left,
                                   right)),
                           SyntaxFactory.ExpressionStatement(
                               SyntaxFactory.AssignmentExpression(
                                   SyntaxKind.SubtractAssignmentExpression,
                                   left,
                                   right))),
                       null,
                       (SyntaxFactoryUtils.Object(), objectName),
                       (SyntaxFactoryUtils.Bool(), subscribedName)));
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(ImmutableArray<(IEventSymbol symbol, int count)> _eventSets, Identifier _runtimeAlias)
        {
            ImmutableArray<ClassDeclarationSyntax> hookClasses = _eventSets
                .SelectIndexed((_s, _i) => CreateEventHook(_s.symbol, _i))
                .ToImmutableArray();
            PropertyDeclarationSyntax? property = SyntaxFactoryUtils.ArrayPropertyOverride(
                    SyntaxFactoryUtils.TupleType(
                        SyntaxFactoryUtils.EventHookType(_runtimeAlias),
                        SyntaxFactoryUtils.Int()),
                    Identifiers.generatedEventHooksName,
                    _eventSets.SelectIndexed((_e, _i)
                        => SyntaxFactory.TupleExpression(
                            SyntaxFactoryUtils.Arguments(
                                SyntaxFactory.ObjectCreationExpression(
                                    SyntaxFactory.IdentifierName(
                                        hookClasses[_i].Identifier))
                                    .WithArgumentList(
                                        SyntaxFactoryUtils.Arguments()),
                                SyntaxFactoryUtils.Literal(_e.count)).Arguments)));
            return hookClasses.Cast<MemberDeclarationSyntax>().Concat(CollectionExtensions.Singleton(property));
        }

        internal static CompilationUnitSyntax Rewrite(Session _session)
        {
            Rewriter rewriter = new(_session.Semantics);
            BlockSyntax newRunMethodBody = (BlockSyntax)rewriter.Visit(_session.RunMethodBody);
            ImmutableArray<(IEventSymbol symbol, int count)> eventSets = rewriter.EventSets;
            if (eventSets.IsEmpty)
            {
                return _session.Syntax;
            }
            IEnumerable<MemberDeclarationSyntax> members = CreateMembers(eventSets, _session.RuntimeAlias);
            return _session.Syntax.ReplaceNode(_session.Class,
                _session.Class
                .ReplaceNode(_session.RunMethodBody, newRunMethodBody)
                .AddMembers(members.ToArray()));
        }

    }

}
