
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using VooDo.AST.Expressions;
using VooDo.AST.Names;

namespace VooDo.WinUI.HookInitializers
{

    public sealed class DependencyPropertyHookInitializer : HookInitializer
    {

        public DependencyPropertyHookInitializer() : base() { }

        public DependencyPropertyHookInitializer(Identifier? _alias) : base(_alias) { }

        protected override Identifier HookTypeName => "DependencyPropertyHook";

        public override Expression? GetInitializer(ISymbol _symbol, CSharpCompilation _compilation)
        {
            INamedTypeSymbol? baseType = _symbol.ContainingType;
            while (baseType is not null && baseType.ToDisplayString() != "Microsoft.UI.Xaml.DependencyObject")
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
