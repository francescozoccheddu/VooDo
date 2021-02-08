using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

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
        public override string ToString() => $"({string.Join(", ", this)})";

        #endregion

    }

}
