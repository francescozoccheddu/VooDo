
using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record IfStatement(Expression Condition, Statement Then, Statement? Else = null) : Statement
    {

        
        public bool HasElse => Else is not null;

        
        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Expression newCondition = (Expression) _map(Condition).NonNull();
            Statement newThen = (Statement) _map(Then).NonNull();
            Statement? newElse = (Statement?) _map(Else);
            if (ReferenceEquals(newCondition, Condition) && ReferenceEquals(newThen, Then) && ReferenceEquals(newElse, Else))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Condition = newCondition,
                    Then = newThen,
                    Else = newElse
                };
            }
        }



        public override IEnumerable<Node> Children => new Node[] { Condition, Then }.Concat(HasElse ? new[] { Else! } : Enumerable.Empty<Node>());
        public override string ToString() => $"{GrammarConstants.ifKeyword} ({Condition})\n"
            + (Then is BlockStatement ? "" : "\t") + Then
            + (Else is null ? "" : $"\n{GrammarConstants.elseKeyword}\n" + (Else is BlockStatement ? "" : "\t") + Else);

        
    }

}
