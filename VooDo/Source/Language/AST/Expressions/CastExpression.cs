using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.Language.AST.Names;
using VooDo.Language.Linking;

namespace VooDo.Language.AST.Expressions
{

    public sealed record CastExpression(ComplexType Type, Expression Expression) : Expression
    {

        #region Overrides

        internal override CastExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.CastExpression(
                Type.EmitNode(_scope, _marker),
                Expression.EmitNode(_scope, _marker))
            .Own(_marker, this);
        public override IEnumerable<ComplexTypeOrExpression> Children => new ComplexTypeOrExpression[] { Expression, Type };
        public override string ToString() => $"({Type}) {Expression}";

        #endregion

    }

}
