using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record ElementAccessExpression(Expression Source, bool Coalesce, ImmutableArray<Expression> Arguments) : AssignableExpression
    {

        #region Delegating constructors

        public ElementAccessExpression(Expression _source, bool _coalesce, params Expression[] _arguments) : this(_source, _coalesce, _arguments.ToImmutableArray()) { }
        public ElementAccessExpression(Expression _source, bool _coalesce, IEnumerable<Expression> _arguments) : this(_source, _coalesce, _arguments.ToImmutableArray()) { }

        #endregion

        #region Members

        private ImmutableArray<Expression> m_arguments = Arguments.NonEmpty();
        public ImmutableArray<Expression> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.NonEmpty();
        }

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Source }.Concat(Arguments);
        public override string ToString() => $"{Source}" + (Coalesce ? "?" : "") + $"[{string.Join(",", Arguments)}]";

        #endregion

    }

}
