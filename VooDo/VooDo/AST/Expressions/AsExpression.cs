
using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record AsExpression(Expression Expression, ComplexType Type) : Expression
    {


        protected override EPrecedence m_Precedence => EPrecedence.Relational;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Expression newExpression = (Expression) _map(Expression).NonNull();
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            if (ReferenceEquals(newExpression, Expression) && ReferenceEquals(newType, Type))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Expression = newExpression,
                    Type = newType
                };
            }
        }


        public override IEnumerable<Node> Children => new Node[] { Expression, Type };
        public override string ToString() => $"{LeftCode(Expression)} {GrammarConstants.asKeyword} {Type}";


    }

}
