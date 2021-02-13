

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record IsExpression(Expression Expression, ComplexType Type, IdentifierOrDiscard? Name = null) : Expression
    {

        #region Members

        public bool IsDeclaration => Name is not null;

        #endregion

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Relational;

        public override IsExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            IdentifierOrDiscard? newName = (IdentifierOrDiscard?) _map(Name);
            if (ReferenceEquals(newType, Type) && ReferenceEquals(newName, Name))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Name = newName
                };
            }
        }

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
        public override IEnumerable<NodeOrIdentifier> Children
            => new NodeOrIdentifier[] { Expression, Type }.Concat(IsDeclaration ? new NodeOrIdentifier[] { Name! } : Enumerable.Empty<NodeOrIdentifier>());
        public override string ToString() => $"{LeftCode(Expression)} {GrammarConstants.isKeyword} {Type}" + (IsDeclaration ? $" {Name}" : "");

        #endregion

    }

}
