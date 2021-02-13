using System;

namespace VooDo.AST.Expressions
{

    public abstract record AssignableExpression : Expression
    {

        public abstract override AssignableExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);

    }

}
