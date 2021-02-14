using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.Compiling;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ConditionalExpression(Expression Condition, Expression True, Expression False) : Expression
    {

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Conditional;

        public override ConditionalExpression ReplaceNodes(Func<Node?, Node?> _map)
        {
            Expression newCondition = (Expression) _map(Condition).NonNull();
            Expression newTrue = (Expression) _map(True).NonNull();
            Expression newFalse = (Expression) _map(False).NonNull();
            if (ReferenceEquals(newCondition, Condition) && ReferenceEquals(newTrue, True) && ReferenceEquals(newFalse, False))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Condition = newCondition,
                    True = True,
                    False = False
                };
            }
        }

        internal override ConditionalExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.ConditionalExpression(
                Condition.EmitNode(_scope, _tagger),
                True.EmitNode(_scope, _tagger),
                False.EmitNode(_scope, _tagger))
            .Own(_tagger, this);
        public override IEnumerable<Expression> Children => new Expression[] { Condition, True, False };
        public override string ToString() => $"{LeftCode(Condition)} ? {True} : {RightCode(False)}";

        #endregion

    }

}
