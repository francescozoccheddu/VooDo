namespace VooDo.Language.AST
{

    public sealed record BinaryExpression(Expression Left, BinaryExpression.EKind Kind, Expression Right) : Expression
    {

        public enum EKind
        {
            Add, Subtract,
            Multiply, Divide, Modulo,
            LeftShift, RightShift,
            Equals, NotEquals,
            LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
            Coalesce,
            LogicAnd, LogicOr,
            BitwiseAnd, BitwiseOr, BitwiseXor
        }

    }

}
