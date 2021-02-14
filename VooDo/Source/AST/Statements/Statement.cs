using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Compiling.Emission;

namespace VooDo.AST.Statements
{

    public abstract record Statement : BodyNode
    {
        internal abstract override StatementSyntax EmitNode(Scope _scope, Tagger _tagger);
    }

}
