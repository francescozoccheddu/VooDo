using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Language.Linking;

namespace VooDo.Language.AST.Directives
{

    public abstract record UsingDirective : Node
    {

        internal abstract override UsingDirectiveSyntax EmitNode(Scope _scope, Marker _marker);

    }

}
