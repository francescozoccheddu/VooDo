using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Compilation.Emission;

namespace VooDo.AST.Statements
{

    public abstract record Statement : Node
    {
        internal abstract override StatementSyntax EmitNode(Scope _scope, Tagger _tagger);
    }

}
