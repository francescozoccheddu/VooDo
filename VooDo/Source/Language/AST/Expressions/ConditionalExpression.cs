using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Language.Linking;

namespace VooDo.Language.AST.Expressions
{

    public sealed record ConditionalExpression(Expression Condition, Expression True, Expression False) : Expression
    {

        #region Overrides

        internal override ConditionalExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.ConditionalExpression(
                Condition.EmitNode(_scope, _marker),
                True.EmitNode(_scope, _marker),
                False.EmitNode(_scope, _marker))
            .Own(_marker, this);
        public override IEnumerable<Expression> Children => new Expression[] { Condition, True, False };
        public override string ToString() => $"{Condition} ? {True} : {False}";

        #endregion

    }

}
