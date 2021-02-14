using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.Compiling;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Expressions
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

        protected override EPrecedence m_Precedence => EPrecedence.Unary;

        public override UnaryExpression ReplaceNodes(Func<Node?, Node?> _map)
        {
            Expression newExpression = (Expression) _map(Expression).NonNull();
            if (ReferenceEquals(newExpression, Expression))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Expression = newExpression
                };
            }
        }

        internal override ExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.PrefixUnaryExpression(
                Kind switch
                {
                    EKind.Plus => SyntaxKind.UnaryPlusExpression,
                    EKind.Minus => SyntaxKind.UnaryMinusExpression,
                    EKind.LogicNot => SyntaxKind.LogicalNotExpression,
                    EKind.BitwiseNot => SyntaxKind.BitwiseNotExpression,
                    _ => throw new InvalidOperationException()
                },
                Expression.EmitNode(_scope, _tagger))
            .Own(_tagger, this);
        public override IEnumerable<Expression> Children => new[] { Expression };
        public override string ToString() => $"{Kind.Token()}{Expression}";

        #endregion

    }

}
