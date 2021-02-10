using Microsoft.CodeAnalysis;

using VooDo.Language.Linking;

namespace VooDo.Language.AST
{

    public abstract record BodyNode
    {

        internal abstract SyntaxNode Emit(Scope _scope, Marker _marker);

    }
}
