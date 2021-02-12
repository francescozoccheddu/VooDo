using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.Compilation;

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
        public override string ToString() => $"{Left} {Kind.Token()} {Right}";

        #endregion

    }

}
