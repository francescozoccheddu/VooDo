namespace VooDo.Language.AST.Expressions
{

    public sealed record UnaryExpression(BinaryExpression.EKind Kind, Expression Expression) : Expression
    {

        public enum EKind
        {
            Plus, Minus,
            LogicNot,
            BitwiseNot
        }

    }

}
