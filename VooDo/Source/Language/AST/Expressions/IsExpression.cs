
using VooDo.Factory.Syntax;

namespace VooDo.Language.AST
{

    public sealed record IsExpression(Expression Expression, ComplexType Type, Identifier? Name = null) : Expression
    {


    }

}
