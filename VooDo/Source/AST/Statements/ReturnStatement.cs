using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Compilation;
using VooDo.Compilation.Emission;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record ReturnStatement(Expression Expression) : Statement
    {

        #region Overrides

        public override ReturnStatement ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

        internal override StatementSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.ReturnStatement(Expression.EmitNode(_scope, _marker)).Own(_marker, this);
        public override IEnumerable<Expression> Children => new[] { Expression };
        public override string ToString() => $"{GrammarConstants.returnKeyword} {Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
