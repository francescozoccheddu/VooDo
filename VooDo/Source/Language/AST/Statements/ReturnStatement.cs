using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Language.AST.Expressions;
using VooDo.Language.Linking;

namespace VooDo.Language.AST.Statements
{

    public sealed record ReturnStatement(Expression Expression) : Statement
    {

        #region Overrides

        internal override StatementSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.ReturnStatement(Expression.EmitNode(_scope, _marker)).Own(_marker, this);
        public override IEnumerable<Expression> Children => new[] { Expression };
        public override string ToString() => $"{GrammarConstants.returnKeyword} {Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
