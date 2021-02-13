using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Expressions;
using VooDo.Compilation;

namespace VooDo.AST.Statements
{

    public sealed record IfStatement(Expression Condition, Statement Then, Statement? Else = null) : Statement
    {

        #region Members

        public bool HasElse => Else is not null;

        #endregion

        #region Overrides

        public override ArrayCreationExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            ImmutableArray<Expression> newSizes = Sizes.Map(_map).NonNull();
            if (ReferenceEquals(newType, Type) && newSizes == Sizes)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Sizes = newSizes
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
