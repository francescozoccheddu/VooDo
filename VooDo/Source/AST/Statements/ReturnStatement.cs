using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record ReturnStatement(Expression Expression) : SingleStatement
    {

        #region Overrides

        public override ReturnStatement ReplaceNodes(Func<Node?, Node?> _map)
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

        internal override StatementSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.ReturnStatement(Expression.EmitNode(_scope, _tagger)).Own(_tagger, this);
        public override IEnumerable<Expression> Children => new[] { Expression };
        public override string ToString() => $"{GrammarConstants.returnKeyword} {Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
