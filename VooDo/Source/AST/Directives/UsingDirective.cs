using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Compiling.Emission;

namespace VooDo.AST.Directives
{

    public abstract record UsingDirective : BodyNode
    {

        public sealed override Script? Parent => (Script?) base.Parent;
        public abstract override UsingDirective ReplaceNodes(Func<Node?, Node?> _map);
        internal abstract override UsingDirectiveSyntax EmitNode(Scope _scope, Tagger _tagger);

    }

}
