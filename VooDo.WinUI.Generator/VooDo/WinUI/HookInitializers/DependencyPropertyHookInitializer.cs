
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.WinUI.Generator;

namespace VooDo.WinUI.HookInitializers
{

    internal sealed class DependencyPropertyHookInitializer : HookInitializer
    {

        internal DependencyPropertyHookInitializer() : base() { }

        internal DependencyPropertyHookInitializer(Identifier? _alias) : base(_alias) { }

        protected override Identifier HookTypeName => Identifiers.dependencyPropertyHookName;

        public override Expression? GetInitializer(ISymbol _symbol, CSharpCompilation _compilation)
        {
            INamedTypeSymbol? baseType = _symbol.ContainingType;
            while (baseType is not null && baseType.ToDisplayString() != Identifiers.dependencyObjectFullName)
            {
                baseType = baseType.BaseType;
            }
            if (baseType is null)
            {
                return null;
            }
            return GetInitializer(_compilation, new ValueArgument(null, new LiteralExpression(_symbol.Name)));
        }

    }

}
