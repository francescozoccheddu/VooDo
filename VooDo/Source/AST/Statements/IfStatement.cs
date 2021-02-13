using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record IfStatement(Expression Condition, Statement Then, Statement? Else = null) : Statement
    {

        #region Members

        public bool HasElse => Else is not null;

        #endregion

        #region Overrides

        public override IfStatement ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

        internal override IfStatementSyntax EmitNode(Scope _scope, Marker _marker)
        {
            ExpressionSyntax condition = Condition.EmitNode(_scope, _marker);
            StatementSyntax then = Then.EmitNode(_scope, _marker);
            return (HasElse
                ? SyntaxFactory.IfStatement(condition, then, SyntaxFactory.ElseClause(Else!.EmitNode(_scope, _marker)))
                : SyntaxFactory.IfStatement(condition, then))
                .Own(_marker, this);
        }

        public override IEnumerable<Node> Children => new Node[] { Condition, Then }.Concat(HasElse ? new[] { Else! } : Enumerable.Empty<Node>());
        public override string ToString() => $"{GrammarConstants.ifKeyword} ({Condition})\n"
            + (Then is BlockStatement ? "" : "\t") + Then
            + (Else is null ? "" : $"\n{GrammarConstants.elseKeyword}\n" + (Else is BlockStatement ? "" : "\t") + Else);

        #endregion

    }

}
