using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Compilation;
using VooDo.Compilation;

namespace VooDo.AST.Statements
{

    public abstract record Statement : Node
    {
        internal abstract override StatementSyntax EmitNode(Scope _scope, Marker _marker);
    }

}
