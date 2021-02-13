using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Compilation;

namespace VooDo.AST.Directives
{

    public abstract record UsingDirective : Node
    {

        public abstract override UsingDirective ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);

        internal abstract override UsingDirectiveSyntax EmitNode(Scope _scope, Marker _marker);

    }

}
