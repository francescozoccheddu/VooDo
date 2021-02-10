using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Language.Linking;

namespace VooDo.Language.AST.Statements
{

    public abstract record Statement : Node
    {
        internal abstract override StatementSyntax EmitNode(Scope _scope, Marker _marker);
    }

}
