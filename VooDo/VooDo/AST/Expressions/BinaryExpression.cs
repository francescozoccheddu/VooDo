
using System;
using System.Collections.Generic;

using VooDo.Utils;

namespace VooDo.AST.Expressions
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

        
        
        protected override EPrecedence m_Precedence => Kind switch
        {
            EKind.Add or
            EKind.Subtract => EPrecedence.Additive,
            EKind.Multiply or
            EKind.Divide or
            EKind.Modulo => EPrecedence.Multiplicative,
            EKind.LeftShift or
            EKind.RightShift => EPrecedence.Shift,
            EKind.Equals or
            EKind.NotEquals => EPrecedence.Equality,
            EKind.LessThan or
            EKind.LessThanOrEqual or
            EKind.GreaterThan or
            EKind.GreaterThanOrEqual => EPrecedence.Relational,
            EKind.Coalesce => EPrecedence.Coalesce,
            EKind.LogicAnd => EPrecedence.LogicAnd,
            EKind.LogicOr => EPrecedence.LogicOr,
            EKind.BitwiseAnd => EPrecedence.BitwiseAnd,
            EKind.BitwiseOr => EPrecedence.BitwiseOr,
            EKind.BitwiseXor => EPrecedence.BitwiseXor,
        };

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Expression newLeft = (Expression) _map(Left).NonNull();
            Expression newRight = (Expression) _map(Right).NonNull();
            if (ReferenceEquals(newLeft, Left) && ReferenceEquals(newRight, Right))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Left = newLeft,
                    Right = newRight
                };
            }
        }


        public override IEnumerable<Node> Children => new Expression[] { Left, Right };
        public override string ToString() => $"{LeftCode(Left)} {Kind.Token()} {RightCode(Right)}";

        
    }

}
