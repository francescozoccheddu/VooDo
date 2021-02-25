
using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record AssignmentStatement(AssignableExpression Target, AssignmentStatement.EKind Kind, Expression Source) : Statement
    {

        
        public enum EKind
        {
            Simple,
            Add, Subtract,
            Multiply, Divide, Modulo,
            LeftShift, RightShift,
            BitwiseAnd, BitwiseOr, BitwiseXor,
            Coalesce
        }

        
        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            AssignableExpression newTarget = (AssignableExpression) _map(Target).NonNull();
            Expression newExpression = (Expression) _map(Source).NonNull();
            if (ReferenceEquals(newTarget, Target) && ReferenceEquals(newExpression, Source))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Target = newTarget,
                    Source = newExpression
                };
            }
        }


        public override IEnumerable<Node> Children => new[] { Target, Source };
        public override string ToString() => $"{Target} {Kind.Token()} {Source}{GrammarConstants.statementEndToken}";

        
    }


}

