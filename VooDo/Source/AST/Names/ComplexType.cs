
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using VooDo.Compilation;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public abstract record ComplexType(bool IsNullable = false, ImmutableArray<ComplexType.RankSpecifier> Ranks = default) : ComplexTypeOrExpression
    {

        #region Creation

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

        protected static TypeSyntax Unwrap(TypeSyntax _type, out bool _nullable, out ImmutableArray<RankSpecifier> _ranks)
        {
            TypeSyntax? newType = _type;
            _nullable = false;
            List<RankSpecifier> ranks = new List<RankSpecifier>();
            while (true)
            {
                if (newType is ArrayTypeSyntax arraytype)
                {
                    if (arraytype.RankSpecifiers.Any(_r => _r.Sizes.Any(_s => _s is not OmittedArraySizeExpressionSyntax)))
                    {
                        throw new ArgumentException("Explicit array size expression", nameof(_type));
                    }
                    ranks.AddRange(arraytype.RankSpecifiers.Select(_r => new RankSpecifier(_r.Rank)));
                    newType = arraytype.ElementType;
                }
                else if (newType is NullableTypeSyntax nullableType)
                {
                    _nullable |= true;
                    newType = nullableType.ElementType;
                }
                else
                {
                    _ranks = ranks.ToImmutableArray();
                    return newType;
                }
            }
        }

        public static ComplexType FromSyntax(TypeSyntax _type, bool _ignoreUnbound = false)
        {
            ComplexType type = Unwrap(_type, out bool nullable, out ImmutableArray<RankSpecifier> ranks) switch
            {
                TupleTypeSyntax tupleType => TupleType.FromSyntax(tupleType, _ignoreUnbound),
                _ => QualifiedType.FromSyntax(_type, _ignoreUnbound),
            };
            return type with
            {
                IsNullable = nullable,
                Ranks = ranks
            };
        }

        public static ComplexType Parse(string _type, bool _ignoreUnbound = false)
            => FromSyntax(SyntaxFactory.ParseTypeName(_type), _ignoreUnbound);

        public static ComplexType FromType(Type _type, bool _ignoreUnbound = false)
        {
            ComplexType type = Unwrap(_type, out bool nullable, out ImmutableArray<RankSpecifier> ranks) switch
            {
                var t when t.IsAssignableTo(typeof(ITuple)) => TupleType.FromType(_type, _ignoreUnbound),
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

        #endregion

        #region Conversion

        public static implicit operator ComplexType(string _type) => Parse(_type);
        public static implicit operator ComplexType(Type _type) => FromType(_type);
        public static implicit operator ComplexType(Identifier _name) => new QualifiedType(new SimpleType(_name));
        public static implicit operator ComplexType(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator string(ComplexType _complexType) => _complexType.ToString();

        #endregion

        #region Nested types

        public sealed record RankSpecifier(int Rank) : Node
        {

            public static implicit operator RankSpecifier(int _rank) => new(_rank);

            private int m_rank = Rank.Assert(_v => _v > 0);
            public int Rank
            {
                get => m_rank;
                init => m_rank = value.Assert(_v => _v > 0);
            }

            public override IEnumerable<NodeOrIdentifier> Children => Enumerable.Empty<NodeOrIdentifier>();
            internal override ArrayRankSpecifierSyntax EmitNode(Scope _scope, Marker _marker)
                => SyntaxFactoryHelper.ArrayRank(m_rank).Own(_marker, this);

            public override string ToString()
                => $"[{new string(',', m_rank - 1)}]";

        }

        #endregion

        #region Members

        private ImmutableArray<RankSpecifier> m_ranks = Ranks.EmptyIfDefault();
        public ImmutableArray<RankSpecifier> Ranks
        {
            get => m_ranks;
            init => m_ranks = value.EmptyIfDefault();

        }
        public bool IsArray => Ranks.Any();

        #endregion

        #region Overrides

        internal sealed override TypeSyntax EmitNode(Scope _scope, Marker _marker)
        {
            TypeSyntax? type = EmitNonArrayNonNullableType(_scope, _marker);
            if (IsNullable)
            {
                type = SyntaxFactory.NullableType(type);
            }
            if (IsArray)
            {
                type = SyntaxFactory.ArrayType(type, Ranks.Select(_r => _r.EmitNode(_scope, _marker)).ToSyntaxList());
            }
            return type.Own(_marker, this);
        }

        private protected abstract TypeSyntax EmitNonArrayNonNullableType(Scope _scope, Marker _marker);

        public override string ToString()
            => $"{(IsNullable ? "?" : "")}{string.Concat(Ranks)}";

        #endregion

    }

}