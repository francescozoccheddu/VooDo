
using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public abstract record ComplexType : ComplexTypeOrExpression
    {

        
        protected static Type Unwrap(Type _type, out bool _nullable, out ImmutableArray<RankSpecifier> _ranks)
        {
            List<RankSpecifier> ranks = new List<RankSpecifier>();
            Type type = _type;
            while (type.IsArray)
            {
                ranks.Add(type.GetArrayRank());
                type = type.GetElementType()!;
            }
            _ranks = ranks.ToImmutableArray();
            Type? nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            _nullable = nullableUnderlyingType is not null;
            if (_nullable)
            {
                type = nullableUnderlyingType!;
            }
            return type;
        }

        public static ComplexType Parse(string _type)
            => Parser.ComplexType(_type);

        public static ComplexType FromType(Type _type, bool _ignoreUnbound = false)
        {
            ComplexType type = Unwrap(_type, out bool nullable, out ImmutableArray<RankSpecifier> ranks) switch
            {
                var t when TupleType.IsTuple(t) => TupleType.FromType(_type, _ignoreUnbound),
                _ => QualifiedType.FromType(_type, _ignoreUnbound)
            };
            return type with
            {
                IsNullable = nullable,
                Ranks = ranks
            };
        }

        public static ComplexType FromType<TType>()
            => FromType(typeof(TType), false);

        
        
        public static implicit operator ComplexType(string _type) => Parse(_type);
        public static implicit operator ComplexType(Type _type) => FromType(_type);
        public static implicit operator ComplexType(Identifier _name) => new QualifiedType(new SimpleType(_name));
        public static implicit operator ComplexType(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator string(ComplexType _complexType) => _complexType.ToString();

        
        
        public sealed record RankSpecifier : BodyNode
        {

            public static implicit operator RankSpecifier(int _rank) => new(_rank);

            public RankSpecifier(int _rank)
            {
                Rank = _rank;
            }

            private int m_rank;
            public int Rank
            {
                get => m_rank;
                init
                {
                    if (value < 1)
                    {
                        throw new SyntaxError(this, "Rank must be positive").AsThrowable();
                    }
                    m_rank = value;
                }
            }

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map) => this;

            public override string ToString()
                => $"[{new string(',', m_rank - 1)}]";

        }

        
        
        public ComplexType(bool _isNullable = false, ImmutableArray<RankSpecifier> _ranks = default)
        {
            IsNullable = _isNullable;
            Ranks = _ranks;
        }

        public bool IsNullable { get; init; }

        private ImmutableArray<RankSpecifier> m_ranks;
        public ImmutableArray<RankSpecifier> Ranks
        {
            get => m_ranks;
            init => m_ranks = value.EmptyIfDefault();

        }
        public bool IsArray => Ranks.Any();

        
        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ImmutableArray<RankSpecifier> newRanks = Ranks.Map(_map).NonNull();
            if (newRanks == Ranks)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Ranks = newRanks
                };
            }
        }

        public override string ToString()
            => $"{(IsNullable ? "?" : "")}{string.Concat(Ranks)}";

        
    }

}