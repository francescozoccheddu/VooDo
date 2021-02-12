using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Compilation;
using VooDo.AST.Names;
using VooDo.Compilation;

namespace VooDo.AST.Expressions
{

    public sealed record DefaultExpression(ComplexType? Type = null) : Expression
    {

        #region Members

        public bool HasType => Type is not null;

        #endregion

        #region Overrides

        internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => (HasType
            ? SyntaxFactory.DefaultExpression(Type!.EmitNode(_scope, _marker))
            : (ExpressionSyntax) SyntaxFactory.LiteralExpression(SyntaxKind.DefaultExpression))
            .Own(_marker, this);
        public override IEnumerable<ComplexType> Children => HasType ? new ComplexType[] { Type! } : Enumerable.Empty<ComplexType>();
        public override string ToString() => GrammarConstants.defaultKeyword + (HasType ? $"({Type})" : "");

        #endregion

    }

}
