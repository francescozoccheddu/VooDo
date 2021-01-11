using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

namespace VooDo.Transformation
{
    public sealed class HookInitializerList : List<IHookInitializerProvider>, IHookInitializerProvider
    {

        public HookInitializerList()
        { }

        public HookInitializerList(IEnumerable<IHookInitializerProvider> _collection) : base(_collection)
        { }

        public HookInitializerList(int _capacity) : base(_capacity)
        { }

        IHookInitializer IHookInitializerProvider.GetHookInitializer(MemberAccessExpressionSyntax _syntax, SemanticModel _semantics)
            => this.Select(_p => _p.GetHookInitializer(_syntax, _semantics)).FirstOrDefault();

        IHookInitializer IHookInitializerProvider.GetHookInitializer(ElementAccessExpressionSyntax _syntax, SemanticModel _semantics)
            => this.Select(_p => _p.GetHookInitializer(_syntax, _semantics)).FirstOrDefault();

    }

}
