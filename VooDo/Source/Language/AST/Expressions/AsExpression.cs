

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Language.AST.Names;
using VooDo.Language.Linking;

namespace VooDo.Language.AST.Expressions
{

    public sealed record AsExpression(Expression Expression, ComplexType Type) : Expression
    {

        #region Overrides

        internal override BinaryExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.BinaryExpression(
                SyntaxKind.AsExpression,
                Expression.EmitNode(_scope, _marker),
                Type.EmitNode(_scope, _marker))
            .Own(_marker, this);
        public override IEnumerable<ComplexTypeOrExpression> Children => new ComplexTypeOrExpression[] { Expression, Type };
        public override string ToString() => $"{Expression} {GrammarConstants.asKeyword} {Type}";

        #endregion

    }

}
