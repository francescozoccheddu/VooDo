
using System;
using System.Collections.Generic;

using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record UnaryExpression(UnaryExpression.EKind Kind, Expression Expression) : Expression
    {

        
        public enum EKind
        {
            Plus, Minus,
            LogicNot,
            BitwiseNot
        }

        
        
        protected override EPrecedence m_Precedence => EPrecedence.Unary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

        public override IEnumerable<Node> Children => new[] { Expression };
        public override string ToString() => $"{Kind.Token()}{Expression}";

        
    }

}
