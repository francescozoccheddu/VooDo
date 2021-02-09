using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Language.Linking;

namespace VooDo.Language.AST.Statements
{

    public abstract record Statement : BodyNode
    {
        internal abstract override StatementSyntax Emit(LinkArguments _arguments);
    }

}
