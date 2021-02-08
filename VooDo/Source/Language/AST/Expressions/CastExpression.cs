namespace VooDo.Language.AST
{

    public sealed record CastExpression(Expression Expression, ComplexType Type) : Expression
    {


    }

}
