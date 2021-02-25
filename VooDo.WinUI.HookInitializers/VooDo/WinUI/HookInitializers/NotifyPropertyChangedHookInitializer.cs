using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.ComponentModel;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;

namespace VooDo.WinUI.HookInitializers
{

    public sealed class NotifyPropertyChangedHookInitializer : HookInitializer
    {

        public NotifyPropertyChangedHookInitializer() : base() { }

        public NotifyPropertyChangedHookInitializer(Identifier? _alias) : base(_alias) { }

        protected override Identifier HookTypeName => "NotifyPropertyChangedHook";

        public override Expression? GetInitializer(ISymbol _symbol, CSharpCompilation _compilation)
        {
            INamedTypeSymbol? type = _symbol.ContainingType;
            string? interfaceName = typeof(INotifyPropertyChanged).FullName;
            if (type is null)
            {
                return null;
            }
            if (type.AllInterfaces.Select(_i => _i.ToDisplayString()).Contains(interfaceName))
            {
                return GetInitializer(_compilation, new ValueArgument(null, new LiteralExpression(_symbol.Name)));
            }
            return null;
        }

    }

}
