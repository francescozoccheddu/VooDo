﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.ComponentModel;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.WinUI.Generator;

namespace VooDo.WinUI.HookInitializers
{

    internal sealed class NotifyPropertyChangedHookInitializer : HookInitializer
    {

        internal NotifyPropertyChangedHookInitializer() : base() { }

        internal NotifyPropertyChangedHookInitializer(Identifier? _alias) : base(_alias) { }

        protected override Identifier HookTypeName => Identifiers.notifyPropertyChangedHookName;

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
