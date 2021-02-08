using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST.Names;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record ObjectCreationExpression(ComplexType? Type, ImmutableArray<Expression> Arguments = default) : Expression
    {

        #region Delegating constructors

        public ObjectCreationExpression(params Expression[] _arguments) : this(_arguments.ToImmutableArray()) { }
        public ObjectCreationExpression(ComplexType? _type, params Expression[] _arguments) : this(_type, _arguments.EmptyIfNull().ToImmutableArray()) { }
        public ObjectCreationExpression(IEnumerable<Expression>? _arguments) : this(_arguments.EmptyIfNull().ToImmutableArray()) { }
        public ObjectCreationExpression(ComplexType? _type, IEnumerable<Expression>? _arguments) : this(_type, _arguments.EmptyIfNull().ToImmutableArray()) { }
        public ObjectCreationExpression(ImmutableArray<Expression> _arguments) : this(null, _arguments) { }

        #endregion

        #region Members

        private ImmutableArray<Expression> m_arguments = Arguments.EmptyIfDefault();
        public ImmutableArray<Expression> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.EmptyIfDefault();
        }
        public bool IsTypeImplicit => Type is null;

        #endregion

        #region Override

        public override IEnumerable<Node> Children => IsTypeImplicit ? Arguments : new Node[] { Type! }.Concat(Arguments);
        public override string ToString() => $"{GrammarConstants.newKeyword} " + (IsTypeImplicit ? $"{Type} " : "") + $"({string.Join(", ", Arguments)})";

        #endregion

    }

}
