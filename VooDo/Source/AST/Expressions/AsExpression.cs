

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record AsExpression(Expression Expression, ComplexType Type) : Expression
    {

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Relational;

        public override AsExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            Expression newExpression = (Expression) _map(Expression).NonNull();
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            if (ReferenceEquals(newExpression, Expression) && ReferenceEquals(newType, Type))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Expression = newExpression,
                    Type = newType
                };
            }
        }

        internal override BinaryExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.BinaryExpression(
                SyntaxKind.AsExpression,
                Expression.EmitNode(_scope, _marker),
                Type.EmitNode(_scope, _marker))
            .Own(_marker, this);
        public override IEnumerable<ComplexTypeOrExpression> Children => new ComplexTypeOrExpression[] { Expression, Type };
        public override string ToString() => $"{LeftCode(Expression)} {GrammarConstants.asKeyword} {Type}";

        #endregion

    }

}
