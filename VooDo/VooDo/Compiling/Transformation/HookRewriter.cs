using Analyzer.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Hooks;
using VooDo.Problems;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.Compiling.Transformation
{

    internal static class HookRewriter
    {

        private sealed class BodyRewriter : CSharpSyntaxRewriter
        {

            private sealed class Entry
            {
                internal Entry(int _index, Expression _initializer)
                {
                    Locations = new HashSet<AbstractLocation>();
                    Initializer = _initializer;
                    Index = _index;
                }

                internal int Index { get; }
                internal HashSet<AbstractLocation> Locations { get; }
                internal Expression Initializer { get; }
                internal int Count { get; set; }
            }

            internal BodyRewriter(SemanticModel _semantics, PointsToAnalysisResult _pointsToAnalysis, IHookInitializer _hookInitializer, CSharpCompilation _compilation)
            {
                m_semantics = _semantics;
                m_pointsToAnalysis = _pointsToAnalysis;
                m_map = new Dictionary<ISymbol, Entry>(SymbolEqualityComparer.Default);
                m_hookInitializer = _hookInitializer;
                m_compilation = _compilation;
            }

            private readonly CSharpCompilation m_compilation;
            private readonly SemanticModel m_semantics;
            private readonly PointsToAnalysisResult m_pointsToAnalysis;
            private readonly Dictionary<ISymbol, Entry> m_map;
            private readonly IHookInitializer m_hookInitializer;

            internal ImmutableArray<(Expression initializer, int count)> Initializers
                => m_map.Values.Select(_e => (_e.Initializer, _e.Count)).ToImmutableArray();

            private ExpressionSyntax? TryReplaceExpression(ExpressionSyntax _expression)
            {
                IOperation sourceOperation = m_semantics.GetOperation(_expression)!;
                if (sourceOperation.Kind is not OperationKind.ArrayElementReference
                    and not OperationKind.FieldReference
                    and not OperationKind.PropertyReference
                    and not OperationKind.LocalReference)
                {
                    return null;
                }
                if (m_semantics.GetTypeInfo(_expression).Type!.IsValueType)
                {
                    return null;
                }
                ISymbol? symbol = m_semantics.GetSymbolInfo((ExpressionSyntax)_expression.Parent!).Symbol;
                if (symbol is null ||
                    (symbol is IFieldSymbol field && (field.IsReadOnly || field.IsConst)))
                {
                    return null;
                }
                PointsToAbstractValue result = m_pointsToAnalysis[sourceOperation.Kind, _expression];
                Entry? entry = m_map.TryGetValue(symbol, out Entry value) ? value : null;
                if (entry is not null && result.Locations.IsSubsetOf(entry.Locations))
                {
                    return null;
                }
                Expression? initializer = entry?.Initializer ?? m_hookInitializer.GetInitializer(symbol, m_compilation);
                if (initializer is null)
                {
                    return null;
                }
                if (entry is null)
                {
                    m_map[symbol] = entry = new Entry(m_map.Count, initializer);
                }
                int hookIndex = entry.Count++;
                if (result.Locations.Count == 1)
                {
                    entry.Locations.Add(result.Locations.First());
                }
                return SyntaxFactoryUtils.SubscribeHookInvocation((ExpressionSyntax)Visit(_expression), entry.Index, hookIndex);
            }

            public override SyntaxNode? VisitElementAccessExpression(ElementAccessExpressionSyntax _node)
            {
                IOperation? operation = m_semantics.GetOperation(_node);
                if (operation is not null && operation.Kind is OperationKind.PropertyReference)
                {
                    ExpressionSyntax? expression = TryReplaceExpression(_node.Expression);
                    if (expression is not null)
                    {
                        return _node.WithExpression(expression);
                    }
                }
                return base.VisitElementAccessExpression(_node);
            }

            public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax _node)
            {
                IOperation? operation = m_semantics.GetOperation(_node);
                if (operation is not null && operation.Kind is OperationKind.FieldReference or OperationKind.PropertyReference)
                {
                    ExpressionSyntax? expression = TryReplaceExpression(_node.Expression);
                    if (expression is not null)
                    {
                        return _node.WithExpression(expression);
                    }
                }
                return base.VisitMemberAccessExpression(_node);
            }

        }

        private static PointsToAnalysisResult CreatePointsToAnalysis(MethodDeclarationSyntax _method, SemanticModel _semantics, CSharpCompilation _compilation, CancellationToken _cancellationToken)
        {
            IMethodSymbol methodSymbol = _semantics.GetDeclaredSymbol(_method, _cancellationToken)!;
            IMethodBodyOperation methodOperation = (IMethodBodyOperation)_semantics.GetOperation(_method, _cancellationToken)!;
            ControlFlowGraph controlFlowGraph = ControlFlowGraph.Create(methodOperation, _cancellationToken);
            AnalyzerOptions options = new AnalyzerOptions(ImmutableArray.Create<AdditionalText>());
            WellKnownTypeProvider typeProvider = WellKnownTypeProvider.GetOrCreate(_compilation);
            InterproceduralAnalysisConfiguration interproceduralAnalysis = InterproceduralAnalysisConfiguration.Create(
                options,
                ImmutableArray.Create<DiagnosticDescriptor>(),
                methodSymbol,
                _compilation,
                InterproceduralAnalysisKind.None,
                _cancellationToken);
            PointsToAnalysisResult? result = PointsToAnalysis.TryGetOrComputeResult(controlFlowGraph, methodSymbol, options, typeProvider, interproceduralAnalysis, null)!;
            if (result is null)
            {
                throw new NoSemanticsProblem().AsThrowable();
            }
            return result;
        }

        private static PropertyDeclarationSyntax CreatePropertySyntax(ImmutableArray<(Expression initializer, int count)> _initializers, Tagger _tagger, Identifier _runtimeAlias)
        {
            ArrayTypeSyntax hookType =
                SyntaxFactoryUtils.SingleArray(
                    SyntaxFactoryUtils.TupleType(
                        SyntaxFactory.ParseTypeName($"{_runtimeAlias}::{typeof(IHook).FullName}"),
                        SyntaxFactoryUtils.PredefinedType(SyntaxKind.IntKeyword)));
            return SyntaxFactoryUtils.ArrowProperty(
                hookType,
                RuntimeHelpers.hooksPropertyName,
                SyntaxFactory.ArrayCreationExpression(
                    hookType,
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        _initializers.Select(_i =>
                            SyntaxFactory.TupleExpression(
                                SyntaxFactoryUtils.Arguments(
                                    Emitter.Emit(_i.initializer, _tagger, _runtimeAlias),
                                    SyntaxFactoryUtils.Literal(_i.count)).Arguments))
                        .ToSeparatedList<ExpressionSyntax>())))
                .WithModifiers(
                    SyntaxFactoryUtils.Tokens(
                        SyntaxKind.ProtectedKeyword,
                        SyntaxKind.OverrideKeyword));
        }

        internal static CompilationUnitSyntax Rewrite(Session _session)
        {
            SemanticModel semantics = _session.Semantics;
            MethodDeclarationSyntax method = _session.Class.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(_m => _m.Identifier.ValueText is RuntimeHelpers.runMethodName or RuntimeHelpers.typedRunMethodName && _m.Modifiers.Any(_d => _d.IsKind(SyntaxKind.OverrideKeyword)))
                .Single();
            PointsToAnalysisResult pointsToAnalysis = CreatePointsToAnalysis(method, semantics, _session.CSharpCompilation!, _session.CancellationToken);
            BodyRewriter rewriter = new BodyRewriter(semantics, pointsToAnalysis, _session.Compilation.Options.HookInitializer, _session.CSharpCompilation);
            BlockSyntax body = (BlockSyntax)rewriter.Visit(method.Body!);
            ImmutableArray<(Expression, int)> initializers = rewriter.Initializers;
            if (initializers.IsEmpty)
            {
                return _session.Syntax;
            }
            _session.EnsureNotCanceled();
            PropertyDeclarationSyntax property = CreatePropertySyntax(rewriter.Initializers, _session.Tagger, _session.RuntimeAlias);
            ClassDeclarationSyntax newClass = _session.Class.ReplaceNode(method.Body!, body);
            newClass = newClass.AddMembers(property);
            return _session.Syntax.ReplaceNode(_session.Class, newClass);
        }

    }

}
