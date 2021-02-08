using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using VooDo.Language.AST.Names;

namespace VooDo.Language.AST
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

        private ImmutableArray<Expression> m_sizes;
        public ImmutableArray<Expression> Sizes
        {
            get => m_sizes;
            init
            {
                if (value.IsDefaultOrEmpty)
                {
                    throw new ArgumentException("Sizes array cannot be empty");
                }
                m_sizes = value;
            }
        }
        public int Rank => Sizes.Length;
        public bool IsTypeImplicit => Type is null;

        #endregion

        #region Override

        public override string ToString() => "new " + (IsTypeImplicit ? $"{Type} " : "") + $"[{string.Join(", ", Sizes)}]";

        #endregion

    }

}
