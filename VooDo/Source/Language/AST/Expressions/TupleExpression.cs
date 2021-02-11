using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record TupleExpression : Expression, IReadOnlyList<Expression>
    {

        #region Members

        private readonly ImmutableArray<Expression> m_expressions;

        public TupleExpression(ImmutableArray<Expression> _expressions)
        {
            if (_expressions.Length < 2)
            {
                throw new ArgumentException("A tuple must have at least two elements", nameof(_expressions));
            }
            m_expressions = _expressions;
        }

        #endregion

        #region Overrides

        public Expression this[int _index] => ((IReadOnlyList<Expression>) m_expressions)[_index];
        public int Count => ((IReadOnlyCollection<Expression>) m_expressions).Count;
        public IEnumerator<Expression> GetEnumerator() => ((IEnumerable<Expression>) m_expressions).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_expressions).GetEnumerator();
        internal override TupleExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.TupleExpression(
                    this.Select(_e => SyntaxFactory.Argument(_e.EmitNode(_scope, _marker)).Own(_marker, _e)).ToSeparatedList())
            .Own(_marker, this);
        public override IEnumerable<Expression> Children => m_expressions;
        public override string ToString() => $"({string.Join(", ", this)})";

        #endregion

    }

}
