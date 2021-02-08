using VooDo.Language.AST.Expressions;

namespace VooDo.Language.AST
{

    public sealed record NameExpression(Expression Expression, ComplexType Type) : AssignableExpression
    {


    }

}
