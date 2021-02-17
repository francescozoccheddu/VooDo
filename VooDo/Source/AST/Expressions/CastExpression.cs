using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record CastExpression(ComplexType Type, Expression Expression) : Expression
    {

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Cast;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            Expression newExpression = (Expression) _map(Expression).NonNull();
            if (ReferenceEquals(newType, Type) && ReferenceEquals(newExpression, Expression))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Expression = newExpression
                };
            }
        }

        internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.CastExpression(
                (TypeSyntax) Type.EmitNode(_scope, _tagger),
                (ExpressionSyntax) Expression.EmitNode(_scope, _tagger))
            .Own(_tagger, this);
        public override IEnumerable<Node> Children => new Node[] { Expression, Type };
        public override string ToString() => $"({Type}) {RightCode(Expression)}";

        #endregion

    }

}
