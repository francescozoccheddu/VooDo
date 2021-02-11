using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Compilation;
using VooDo.Language.AST.Expressions;
using VooDo.Language.Linking;

namespace VooDo.Language.AST.Statements
{

    public sealed record IfStatement(Expression Condition, Statement Then, Statement? Else = null) : Statement
    {

        #region Members

        public bool HasElse => Else is not null;

        #endregion

        #region Overrides

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
