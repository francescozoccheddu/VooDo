using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;

using System;
using System.Collections.Generic;

using VooDo.Hooks;
using VooDo.AST.Expressions;

namespace VooDo.Compilation
{

    internal sealed class HookContext
    {

        private sealed class SymbolData
        {

            internal SymbolData(IHookInitializer _initializer) => Initializer = _initializer;

            internal HashSet<AbstractLocation> Locations { get; } = new HashSet<AbstractLocation>();
            internal IHookInitializer Initializer { get; }

        }

        private readonly PointsToAnalysis m_pointsToAnalysis;
        private readonly Dictionary<ISymbol, SymbolData> m_symbolMap;
        private readonly SemanticModel m_semantics;

        internal HookContext(SemanticModel _semantics)
        {
            throw new NotImplementedException();
        }

        public IHookInitializer? CreateHook(ElementAccessExpression _expression, ElementAccessExpressionSyntax _syntax)
        {
            throw new NotImplementedException();
        }

        public IHookInitializer? CreateHook(MemberAccessExpression _expression, MemberAccessExpressionSyntax _syntax)
        {
            throw new NotImplementedException();
        }

    }

}
