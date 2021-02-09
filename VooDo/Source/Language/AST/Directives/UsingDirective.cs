using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Language.Linking;

namespace VooDo.Language.AST.Directives
{

    public abstract record UsingDirective : BodyNode
    {

        internal abstract override UsingDirectiveSyntax Emit(LinkArguments _arguments);

    }

}
