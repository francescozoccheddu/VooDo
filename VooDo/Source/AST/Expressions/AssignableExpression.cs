using System;

namespace VooDo.AST.Expressions
{

    public abstract record AssignableExpression : Expression
    {

        public abstract override AssignableExpression ReplaceNodes(Func<Node?, Node?> _map);

    }

}
