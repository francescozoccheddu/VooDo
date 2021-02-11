using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Names
{

    public abstract record ComplexType(bool IsNullable = false, ImmutableArray<int> Ranks = default) : ComplexTypeOrExpression
    {

        #region Creation

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
            TypeSyntax? newType = _type;
            _nullable = false;
            List<int> ranks = new List<int>();
            while (true)
            {
                if (newType is ArrayTypeSyntax arraytype)
                {
                    if (arraytype.RankSpecifiers.Any(_r => _r.Sizes.Any(_s => _s is not OmittedArraySizeExpressionSyntax)))
                    {
                        throw new ArgumentException("Explicit array size expression", nameof(_type));
                    }
                    ranks.AddRange(arraytype.RankSpecifiers.Select(_r => _r.Rank));
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
            ComplexType type = Unwrap(_type, out bool nullable, out ImmutableArray<int> ranks) switch
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
            ComplexType type = Unwrap(_type, out bool nullable, out ImmutableArray<int> ranks) switch
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

        #region Members

        private ImmutableArray<int> m_ranks = Ranks.EmptyIfDefault().AssertAll<ImmutableArray<int>, int>(_r => _r > 0);
        public ImmutableArray<int> Ranks
        {
            get => m_ranks;
            init => m_ranks = value.EmptyIfDefault().AssertAll<ImmutableArray<int>, int>(_r => _r > 0);

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
                IEnumerable<ArrayRankSpecifierSyntax> ranks = Ranks.Select(_r
                    => SyntaxFactory.ArrayRankSpecifier(
                            Enumerable.Repeat(SyntaxFactory.OmittedArraySizeExpression(), _r)
                            .ToSeparatedList<ExpressionSyntax>()));
                type = SyntaxFactory.ArrayType(type, ranks.ToSyntaxList());
            }
            return type.Own(_marker, this);
        }

        internal abstract TypeSyntax EmitNonArrayNonNullableType(Scope _scope, Marker _marker);

        public override string ToString()
            => $"{(IsNullable ? "?" : "")}{string.Concat(Ranks.Select(_r => $"[{new string(',', _r - 1)}]"))}";

        #endregion

    }

}