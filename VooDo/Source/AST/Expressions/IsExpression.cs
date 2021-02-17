

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
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

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

        internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
            => (IsDeclaration
            ? SyntaxFactory.IsPatternExpression(
                (ExpressionSyntax) Expression.EmitNode(_scope, _tagger),
                SyntaxFactory.DeclarationPattern(
                    (TypeSyntax) Type.EmitNode(_scope, _tagger),
                    (VariableDesignationSyntax) Name!.EmitNode(_scope, _tagger)))
            : (ExpressionSyntax) SyntaxFactory.BinaryExpression(
                SyntaxKind.IsExpression,
                (ExpressionSyntax) Expression.EmitNode(_scope, _tagger),
                (ExpressionSyntax) Type.EmitNode(_scope, _tagger)))
            .Own(_tagger, this);
        public override IEnumerable<Node> Children
            => new Node[] { Expression, Type }.Concat(IsDeclaration ? new Node[] { Name! } : Enumerable.Empty<Node>());
        public override string ToString() => $"{LeftCode(Expression)} {GrammarConstants.isKeyword} {Type}" + (IsDeclaration ? $" {Name}" : "");

        #endregion

    }

}
