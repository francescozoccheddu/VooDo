

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Language.AST.Names;
using VooDo.Language.Linking;

namespace VooDo.Language.AST.Expressions
{

    public sealed record IsExpression(Expression Expression, ComplexType Type, IdentifierOrDiscard? Name = null) : Expression
    {

        #region Members

        public bool IsDeclaration => Name is not null;

        #endregion

        #region Overrides

        internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => (IsDeclaration
            ? SyntaxFactory.IsPatternExpression(
                Expression.EmitNode(_scope, _marker),
                SyntaxFactory.DeclarationPattern(
                    Type.EmitNode(_scope, _marker),
                    Name!.EmitNode(_scope, _marker)))
            : (ExpressionSyntax) SyntaxFactory.BinaryExpression(
                SyntaxKind.IsExpression,
                Expression.EmitNode(_scope, _marker),
                Type.EmitNode(_scope, _marker)))
            .Own(_marker, this);
        public override IEnumerable<BodyNodeOrIdentifier> Children
            => new BodyNodeOrIdentifier[] { Expression, Type }.Concat(IsDeclaration ? new BodyNodeOrIdentifier[] { Name! } : Enumerable.Empty<BodyNodeOrIdentifier>());
        public override string ToString() => $"{Expression} {GrammarConstants.isKeyword} {Type}" + (IsDeclaration ? $" {Name}" : "");

        #endregion

    }

}
