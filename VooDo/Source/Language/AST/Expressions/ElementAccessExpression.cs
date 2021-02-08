using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace VooDo.Language.AST.Expressions
{

    public sealed record ElementAccessExpression(Expression Source, bool Coalesce, ImmutableArray<Expression> Arguments) : AssignableExpression
    {

        #region Delegating constructors

        public ElementAccessExpression(Expression _source, bool _coalesce, params Expression[] _arguments) : this(_source, _coalesce, _arguments.ToImmutableArray()) { }
        public ElementAccessExpression(Expression _source, bool _coalesce, IEnumerable<Expression> _arguments) : this(_source, _coalesce, _arguments.ToImmutableArray()) { }

        #endregion

        #region Members

        private ImmutableArray<Expression> m_arguments;
        public ImmutableArray<Expression> Arguments
        {
            get => m_arguments;
            init
            {
                if (m_arguments.IsDefaultOrEmpty)
                {
                    throw new ArgumentException("Argument list cannot be empty");
                }
                m_arguments = value;
            }
        }

        #endregion

        #region Overrides

        public override string ToString() => $"{Source}" + (Coalesce ? "?." : ".") + $"[{string.Join(",", Arguments)}]";

        #endregion

    }

}
