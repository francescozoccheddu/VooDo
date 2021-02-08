using System.Collections.Generic;

namespace VooDo.Language.AST.Expressions
{

    public sealed record BinaryExpression(Expression Left, BinaryExpression.EKind Kind, Expression Right) : Expression
    {

        #region Nested types

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

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Left, Right };
        public override string ToString() => $"{Left} {Kind.Token()} {Right}";

        #endregion

    }

}
