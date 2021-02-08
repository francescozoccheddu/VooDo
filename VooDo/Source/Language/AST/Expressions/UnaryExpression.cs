namespace VooDo.Language.AST.Expressions
{

    public sealed record UnaryExpression(BinaryExpression.EKind Kind, Expression Expression) : Expression
    {

        #region Nested types

        public enum EKind
        {
            Plus, Minus,
            LogicNot,
            BitwiseNot
        }

        #endregion

        #region Overrides

        public override string ToString() => $"{Kind.Token()}{Expression}";

        #endregion

    }

}
