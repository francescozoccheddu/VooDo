using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Compilation;
using VooDo.Compilation;

namespace VooDo.AST.Directives
{

    public abstract record UsingDirective : Node
    {

        internal abstract override UsingDirectiveSyntax EmitNode(Scope _scope, Marker _marker);

    }

}
