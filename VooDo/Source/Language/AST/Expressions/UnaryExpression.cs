namespace VooDo.Language.AST
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
