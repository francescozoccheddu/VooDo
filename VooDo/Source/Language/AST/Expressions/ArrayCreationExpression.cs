using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.AST.Names;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record ArrayCreationExpression(ComplexType? Type, ImmutableArray<Expression> Sizes) : Expression
    {

        #region Delegating constructors

        public ArrayCreationExpression(params Expression[] _sizes) : this(_sizes.ToImmutableArray()) { }
        public ArrayCreationExpression(ComplexType? _type, params Expression[] _sizes) : this(_type, _sizes.ToImmutableArray()) { }
        public ArrayCreationExpression(IEnumerable<Expression> _sizes) : this(_sizes.ToImmutableArray()) { }
        public ArrayCreationExpression(ComplexType? _type, IEnumerable<Expression> _sizes) : this(_type, _sizes.ToImmutableArray()) { }
        public ArrayCreationExpression(ImmutableArray<Expression> _sizes) : this(null, _sizes) { }

        #endregion

        #region Members

        private ImmutableArray<Expression> m_sizes = Sizes.NonEmpty();
        public ImmutableArray<Expression> Sizes
        {
            get => m_sizes;
            init => m_sizes = Sizes.NonEmpty();
        }
        public int Rank => Sizes.Length;
        public bool IsTypeImplicit => Type is null;

        #endregion

        #region Override

        public override IEnumerable<Node> Children => IsTypeImplicit ? Sizes : new Node[] { Type! }.Concat(Sizes);
        public override string ToString() => $"{GrammarConstants.newKeyword} " + (IsTypeImplicit ? $"{Type} " : "") + $"[{string.Join(", ", Sizes)}]";

        #endregion

    }

}
