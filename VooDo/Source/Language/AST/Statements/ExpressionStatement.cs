﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.Language.AST.Expressions;
using VooDo.Language.Linking;

namespace VooDo.Language.AST.Statements
{

    public sealed record ExpressionStatement(Expression Expression) : Statement
    {

        #region Overrides

        internal override ExpressionStatementSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.ExpressionStatement(Expression.EmitNode(_scope, _marker)).Own(_marker, this);
        public override IEnumerable<Expression> Children => new[] { Expression };
        public override string ToString() => $"{Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
