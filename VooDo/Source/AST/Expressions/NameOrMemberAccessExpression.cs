using System;

namespace VooDo.AST.Expressions
{

    public abstract record NameOrMemberAccessExpression : AssignableExpression
    {

        public abstract override NameOrMemberAccessExpression ReplaceNodes(Func<Node?, Node?> _map);

    }

}
