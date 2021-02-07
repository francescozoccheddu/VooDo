using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using VooDo.Utils;

namespace VooDo.Factory.Syntax
{
    public abstract class ComplexType
    {

        protected static Type Unwrap(Type _type, out bool _nullable, out ImmutableArray<int> _ranks)
        {
            List<int> ranks = new List<int>();
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

        protected static TypeSyntax Unwrap(TypeSyntax _type, out bool _nullable, out ImmutableArray<int> _ranks)
        {
            _nullable = false;
            _ranks = default;
            switch (_type)
            {
                case ArrayTypeSyntax arraytype:
                {
                    if (arraytype.RankSpecifiers.Any(_r => _r.Sizes.Any(_s => _s is not OmittedArraySizeExpressionSyntax)))
                    {
                        throw new ArgumentException("Explicit array size expression", nameof(_type));
                    }
                    _ranks = arraytype.RankSpecifiers.Select(_r => _r.Rank).ToImmutableArray();
                    return arraytype.ElementType;
                }
                case NullableTypeSyntax nullableType:
                _nullable = true;
                return nullableType.ElementType;
                default:
                return _type;
            }
        }

        public static ComplexType FromSyntax(TypeSyntax _type, bool _ignoreUnbound = false)
            => ((ComplexType) (Unwrap(_type, out bool nullable, out ImmutableArray<int> ranks) switch
            {
                TupleTypeSyntax tupleType => TupleType.FromSyntax(tupleType, _ignoreUnbound),
                _ => QualifiedType.FromSyntax(_type, _ignoreUnbound),
            })).WithIsNullable(nullable).WithRanks(ranks);

        public static ComplexType Parse(string _type, bool _ignoreUnbound = false)
            => FromSyntax(SyntaxFactory.ParseTypeName(_type), _ignoreUnbound);

        public static ComplexType FromType(Type _type, bool _ignoreUnbound = false)
        {
            ComplexType type = Unwrap(_type, out bool nullable, out ImmutableArray<int> ranks) switch
            {
                var t when t.IsAssignableTo(typeof(ITuple)) => TupleType.FromType(_type, _ignoreUnbound),
                _ => QualifiedType.FromType(_type, _ignoreUnbound)
            };
            return type.WithIsNullable(nullable).WithRanks(ranks);
        }

        public static ComplexType FromType<TType>()
            => FromType(typeof(TType), false);

        public static implicit operator ComplexType(Identifier _identifier) => new QualifiedType(_identifier);
        public static implicit operator ComplexType(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator ComplexType(string _type) => Parse(_type);
        public static implicit operator ComplexType(Type _type) => FromType(_type);

        internal ComplexType(IEnumerable<int>? _ranks = null) : this(false, _ranks) { }

        internal ComplexType(bool _isNullable = false, IEnumerable<int>? _ranks = null)
        {
            IsNullable = _isNullable;
            Ranks = _ranks.EmptyIfNull().ToImmutableArray();
            if (Ranks.Any(_i => _i < 1))
            {
                throw new ArgumentException("Non positive rank", nameof(_ranks));
            }
        }

        public bool IsNullable { get; }
        public ImmutableArray<int> Ranks { get; }
        public bool IsArray => Ranks.Any();

        public abstract ComplexType WithIsNullable(bool _nullable);

        public ComplexType WithRanks(int[] _ranks)
            => WithRanks((IEnumerable<int>) _ranks);
        public abstract ComplexType WithRanks(IEnumerable<int> _ranks);

        public override bool Equals(object? _obj) => _obj is ComplexType other && IsNullable == other.IsNullable && Ranks.Equals(other.Ranks);
        public override int GetHashCode() => Identity.CombineHash(IsNullable, Identity.CombineHashes(Ranks));
        public static bool operator ==(ComplexType? _left, ComplexType? _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(ComplexType? _left, ComplexType? _right) => !(_left == _right);

        public override string ToString()
            => $"{(IsNullable ? "?" : "")}{string.Concat(Ranks.Select(_r => $"[{new string(',', _r - 1)}]"))}";

    }
}