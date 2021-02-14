using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Compilation.Emission;

namespace VooDo.AST.Expressions
{

    public sealed record DefaultExpression(ComplexType? Type = null) : Expression
    {

        #region Members

        public bool HasType => Type is not null;

        #endregion

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        public override DefaultExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ComplexType? newType = (ComplexType?) _map(Type);
            if (ReferenceEquals(newType, Type))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType
                };
            }
        }

        internal override ExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
            => (HasType
            ? SyntaxFactory.DefaultExpression(Type!.EmitNode(_scope, _tagger))
            : (ExpressionSyntax) SyntaxFactory.LiteralExpression(SyntaxKind.DefaultExpression))
            .Own(_tagger, this);
        public override IEnumerable<ComplexType> Children => HasType ? new ComplexType[] { Type! } : Enumerable.Empty<ComplexType>();
        public override string ToString() => GrammarConstants.defaultKeyword + (HasType ? $"({Type})" : "");

        #endregion

    }

}
