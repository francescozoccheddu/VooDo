using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Expressions
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
            _ => throw new InvalidOperationException(),
        };

        public override BinaryExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

        internal override BinaryExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.BinaryExpression(
                Kind switch
                {
                    EKind.Add => SyntaxKind.AddExpression,
                    EKind.Subtract => SyntaxKind.SubtractExpression,
                    EKind.Multiply => SyntaxKind.MultiplyExpression,
                    EKind.Divide => SyntaxKind.DivideExpression,
                    EKind.Modulo => SyntaxKind.ModuloExpression,
                    EKind.LeftShift => SyntaxKind.LeftShiftExpression,
                    EKind.RightShift => SyntaxKind.RightShiftExpression,
                    EKind.Equals => SyntaxKind.EqualsExpression,
                    EKind.NotEquals => SyntaxKind.NotEqualsExpression,
                    EKind.LessThan => SyntaxKind.LessThanExpression,
                    EKind.LessThanOrEqual => SyntaxKind.LessThanOrEqualExpression,
                    EKind.GreaterThan => SyntaxKind.GreaterThanExpression,
                    EKind.GreaterThanOrEqual => SyntaxKind.GreaterThanOrEqualExpression,
                    EKind.Coalesce => SyntaxKind.CoalesceExpression,
                    EKind.LogicAnd => SyntaxKind.LogicalAndExpression,
                    EKind.LogicOr => SyntaxKind.LogicalOrExpression,
                    EKind.BitwiseAnd => SyntaxKind.BitwiseAndExpression,
                    EKind.BitwiseOr => SyntaxKind.BitwiseOrExpression,
                    EKind.BitwiseXor => SyntaxKind.ExclusiveOrExpression,
                    _ => throw new InvalidOperationException(),
                },
                Left.EmitNode(_scope, _marker),
                Right.EmitNode(_scope, _marker))
            .Own(_marker, this);
        public override IEnumerable<Expression> Children => new Expression[] { Left, Right };
        public override string ToString() => $"{LeftCode(Left)} {Kind.Token()} {RightCode(Right)}";

        #endregion

    }

}
