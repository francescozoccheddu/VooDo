using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.Language.Linking;

namespace VooDo.Language.AST.Expressions
{

    public sealed record UnaryExpression(UnaryExpression.EKind Kind, Expression Expression) : Expression
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

        internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.PrefixUnaryExpression(
                Kind switch
                {
                    EKind.Plus => SyntaxKind.UnaryPlusExpression,
                    EKind.Minus => SyntaxKind.UnaryMinusExpression,
                    EKind.LogicNot => SyntaxKind.LogicalNotExpression,
                    EKind.BitwiseNot => SyntaxKind.BitwiseNotExpression,
                    _ => throw new InvalidOperationException()
                },
                Expression.EmitNode(_scope, _marker))
            .Own(_marker, this);
        public override IEnumerable<Expression> Children => new[] { Expression };
        public override string ToString() => $"{Kind.Token()}{Expression}";

        #endregion

    }

}
