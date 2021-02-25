
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ArrayCreationExpression : Expression
    {

        
        public ArrayCreationExpression(ComplexType _type, ImmutableArray<Expression> _sizes)
        {
            m_type = Type = _type;
            m_sizes = Sizes = _sizes;
        }

        private ComplexType m_type;
        public ComplexType Type
        {
            get => m_type;
            init
            {
                if (!value.IsArray)
                {
                    throw new ChildSyntaxError(this, value, "Array creation type must be array").AsThrowable();
                }
                m_type = value;
            }
        }
        private ImmutableArray<Expression> m_sizes;
        public ImmutableArray<Expression> Sizes
        {
            get => m_sizes;
            init
            {
                if (value.IsDefaultOrEmpty)
                {
                    throw new SyntaxError(this, "Array creation sizes array cannot be empty").AsThrowable();
                }
                m_sizes = value.EmptyIfDefault();
            }
        }
        public int Rank => Sizes.Length;

        
        
        protected override IEnumerable<Problem> GetSelfSyntaxProblems()
        {
            if (Type.Ranks[0].Rank != Sizes.Length)
            {
                yield return new SyntaxError(this, "Type first rank does not match the number of sizes");
            }
        }

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            ImmutableArray<Expression> newSizes = Sizes.Map(_map).NonNull();
            if (ReferenceEquals(newType, Type) && newSizes == Sizes)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Sizes = newSizes
                };
            }
        }


        public override IEnumerable<Node> Children => new ComplexTypeOrExpression[] { Type }.Concat(Sizes);
        public override string ToString() => $"{GrammarConstants.newKeyword} {Type with { Ranks = default }}[{string.Join(", ", Sizes)}]";

        
    }

}
