
using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record ExpressionStatement(InvocationOrObjectCreationExpression Expression) : Statement
    {

        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            InvocationOrObjectCreationExpression newType = (InvocationOrObjectCreationExpression) _map(Expression).NonNull();
            if (ReferenceEquals(newType, Expression))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Expression = newType
                };
            }
        }

        public override IEnumerable<Node> Children => new Node[] { Expression };
        public override string ToString() => $"{Expression}{GrammarConstants.statementEndToken}";

        
    }

}
