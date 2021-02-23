
using System;
using System.Collections.Generic;

using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ConditionalExpression(Expression Condition, Expression True, Expression False) : Expression
    {

        
        protected override EPrecedence m_Precedence => EPrecedence.Conditional;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

        public override IEnumerable<Node> Children => new Expression[] { Condition, True, False };
        public override string ToString() => $"{LeftCode(Condition)} ? {True} : {RightCode(False)}";

        
    }

}
