using Microsoft.CodeAnalysis.CSharp.Syntax;

using VooDo.Language.Linking;

namespace VooDo.Language.AST.Expressions
{

    public abstract record Expression : ComplexTypeOrExpression
    {

        internal abstract override ExpressionSyntax Emit(LinkArguments _arguments);

    }

}
