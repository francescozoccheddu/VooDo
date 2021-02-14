using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Expressions;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Statements
{

    public sealed record ExpressionStatement(InvocationOrObjectCreationExpression Expression) : Statement
    {

        #region Overrides

        public override ExpressionStatement ReplaceNodes(Func<Node?, Node?> _map)
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

        internal override ExpressionStatementSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.ExpressionStatement(Expression.EmitNode(_scope, _tagger)).Own(_tagger, this);
        public override IEnumerable<InvocationOrObjectCreationExpression> Children => new[] { Expression };
        public override string ToString() => $"{Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
