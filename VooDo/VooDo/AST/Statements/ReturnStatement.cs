
using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record ReturnStatement(Expression Expression) : Statement
    {

        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Expression newType = (Expression) _map(Expression).NonNull();
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


        public override IEnumerable<Node> Children => new[] { Expression };
        public override string ToString() => $"{GrammarConstants.returnKeyword} {Expression}{GrammarConstants.statementEndToken}";

        
    }

}
