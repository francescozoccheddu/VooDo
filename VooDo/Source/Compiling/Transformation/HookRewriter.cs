using Analyzer.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using VooDo.Hooks;
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
                internal Entry(IHookInitializer _initializer)
                {
                    Locations = new HashSet<AbstractLocation>();
                    Initializer = _initializer;
                }

                internal HashSet<AbstractLocation> Locations { get; }
                internal IHookInitializer Initializer { get; }
            }

            internal BodyRewriter(SemanticModel _semantics, PointsToAnalysisResult _pointsToAnalysis, IHookInitializerProvider _hookInitializerProvider)
            {
                m_semantics = _semantics;
                m_pointsToAnalysis = _pointsToAnalysis;
                m_map = new Dictionary<ISymbol, Entry>(SymbolEqualityComparer.Default);
                m_hookInitializerProvider = _hookInitializerProvider;
            }

            private readonly SemanticModel m_semantics;
            private readonly PointsToAnalysisResult m_pointsToAnalysis;
            private readonly Dictionary<ISymbol, Entry> m_map;
            private readonly IHookInitializerProvider m_hookInitializerProvider;
            private readonly List<IHookInitializer> m_hookInitializers = new List<IHookInitializer>();

            public override SyntaxNode? VisitElementAccessExpression(ElementAccessExpressionSyntax _node)
            {
                return base.VisitElementAccessExpression(_node);
            }

            public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax _node)
            {
                IOperation operation = m_semantics.GetOperation(_node)!;
                if (operation.Kind is not OperationKind.FieldReference
                    and not OperationKind.PropertyReference)
                {
                    goto skip;
                }
                IOperation sourceOperation = m_semantics.GetOperation(_node.Expression)!;
                if (sourceOperation.Kind is not OperationKind.ArrayElementReference
                    and not OperationKind.FieldReference
                    and not OperationKind.PropertyReference
                    and not OperationKind.LocalReference)
                {
                    goto skip;
                }
                if (m_semantics.GetTypeInfo(_node.Expression).Type!.IsValueType)
                {
                    goto skip;
                }
                ISymbol? symbol = m_semantics.GetSymbolInfo(_node).Symbol;
                if (symbol is null ||
                    (symbol is IFieldSymbol field && (field.IsReadOnly || field.IsConst)))
                {
                    goto skip;
                }
                PointsToAbstractValue result = m_pointsToAnalysis[sourceOperation];
                Entry? entry = m_map.GetValueOrDefault(symbol);
                if (entry is not null && result.Locations.All(_l => entry.Locations.Contains(_l)))
                {
                    goto skip;
                }
                IHookInitializer? initializer = entry?.Initializer ?? m_hookInitializerProvider.Provide(symbol);
                if (initializer is null)
                {
                    goto skip;
                }
                InvocationExpressionSyntax expression = SyntaxFactoryUtils.SubscribeHookInvocation((ExpressionSyntax) Visit(_node.Expression), m_hookInitializers.Count);
                m_hookInitializers.Add(initializer);
                return _node.WithExpression(expression);
                skip:
                return base.VisitMemberAccessExpression(_node);
            }

        }

        private static PointsToAnalysisResult CreatePointsToAnalysis(MethodDeclarationSyntax _method, SemanticModel _semantics, CSharpCompilation _compilation)
        {
            IMethodSymbol methodSymbol = _semantics.GetDeclaredSymbol(_method)!;
            IMethodBodyOperation methodOperation = (IMethodBodyOperation) _semantics.GetOperation(_method)!;
            ControlFlowGraph controlFlowGraph = ControlFlowGraph.Create(methodOperation);
            AnalyzerOptions options = new AnalyzerOptions(ImmutableArray.Create<AdditionalText>());
            WellKnownTypeProvider typeProvider = WellKnownTypeProvider.GetOrCreate(_compilation);
            InterproceduralAnalysisConfiguration interproceduralAnalysis = InterproceduralAnalysisConfiguration.Create(
                options,
                ImmutableArray.Create<DiagnosticDescriptor>(),
                methodSymbol,
                _compilation,
                InterproceduralAnalysisKind.None,
                CancellationToken.None);
            return PointsToAnalysis.TryGetOrComputeResult(controlFlowGraph, methodSymbol, options, typeProvider, interproceduralAnalysis, null)!;
        }

        internal static CompilationUnitSyntax Rewrite(Session _session)
        {
            SemanticModel semantics = _session.Semantics!;
            CompilationUnitSyntax root = _session.Syntax!;
            NamespaceDeclarationSyntax namespaceDeclaration = root.Members.OfType<NamespaceDeclarationSyntax>().Single();
            ClassDeclarationSyntax classDeclaration = namespaceDeclaration.Members.OfType<ClassDeclarationSyntax>().Single();
            MethodDeclarationSyntax method = classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(_m => _m.Identifier.ValueText is nameof(Program.Run) or nameof(TypedProgram<object>.TypedRun) && _m.Modifiers.Any(_d => _d.IsKind(SyntaxKind.OverrideKeyword)))
                .Single();
            PointsToAnalysisResult pointsToAnalysis = CreatePointsToAnalysis(method, semantics, _session.CSharpCompilation!);
            BodyRewriter rewriter = new BodyRewriter(semantics, pointsToAnalysis, _session.Compilation.Options.HookInitializerProvider);
            BlockSyntax body = (BlockSyntax) rewriter.Visit(method.Body!);
            throw new NotImplementedException();
        }

    }

}
