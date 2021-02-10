using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VooDo.Language.AST
{

    public abstract record ComplexTypeOrExpression : BodyNode
    {

        internal abstract override ExpressionSyntax Emit(LinkArguments _arguments);

    }

}
