﻿#nullable enable

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Factory.Syntax
{
    public sealed class QualifiedType : IEquatable<QualifiedType>
    {

        public static QualifiedType FromSyntax(TypeSyntax _type)
        {
            switch (_type)
            {
                case ArrayTypeSyntax arraytype:
                {
                    if (arraytype.RankSpecifiers.Any(_r => _r.Sizes.Any(_s => _s is not OmittedArraySizeExpressionSyntax)))
                    {
                        throw new ArgumentException("Explicit array size expression", nameof(_type));
                    }
                    IEnumerable<int> ranks = arraytype.RankSpecifiers.Select(_r => _r.Rank);
                    return FromSyntax(arraytype.ElementType).WithRanks(ranks);
                }

                case NullableTypeSyntax nullableType:
                return FromSyntax(nullableType.ElementType).WithIsNullable(true);
                case QualifiedNameSyntax qualifiedNameSyntax:
                {
                    QualifiedType left = FromSyntax(qualifiedNameSyntax.Left);
                    return left.WithPath(left.Path.Add(SimpleType.FromSyntax(qualifiedNameSyntax.Right)));
                }

                case AliasQualifiedNameSyntax aliasQualifiedNameSyntax:
                return new QualifiedType(Identifier.FromSyntax(aliasQualifiedNameSyntax.Alias.Identifier), SimpleType.FromSyntax(aliasQualifiedNameSyntax.Name));
                default:
                return SimpleType.FromSyntax(_type);
            }
        }

        public static QualifiedType Parse(string _type)
            => FromSyntax(SyntaxFactory.ParseTypeName(_type));

        public static QualifiedType FromType(Type _type)
        {
            if (_type.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Unbound type", nameof(_type));
            }
            if (_type.IsGenericParameter)
            {
                throw new ArgumentException("Generic parameter type", nameof(_type));
            }
            if (_type.IsPointer)
            {
                throw new ArgumentException("Pointer type", nameof(_type));
            }
            if (_type.IsByRef)
            {
                throw new ArgumentException("Ref type", nameof(_type));
            }
            if (_type == typeof(void))
            {
                throw new ArgumentException("Void type", nameof(_type));
            }
            if (_type.IsPrimitive)
            {
                return new QualifiedType(SimpleType.FromType(_type));
            }
            else
            {
                List<int> ranks = new List<int>();
                Type type = _type;
                while (type.IsArray)
                {
                    ranks.Add(type.GetArrayRank());
                    type = type.GetElementType();
                }
                Type nullableUnderlyingType = Nullable.GetUnderlyingType(type);
                bool nullable = nullableUnderlyingType != null;
                if (nullable)
                {
                    type = nullableUnderlyingType!;
                }
                List<SimpleType> path = new List<SimpleType>
                {
                    SimpleType.FromType(type)
                };
                while (type.IsNested)
                {
                    type = type.DeclaringType;
                    path.Add(SimpleType.FromType(type));
                }
                path.Reverse();
                ranks.Reverse();
                return new QualifiedType((Namespace) type.Namespace, path, nullable, ranks);
            }
        }

        public static QualifiedType FromType<TType>()
            => FromType(typeof(TType));

        public static implicit operator QualifiedType(Identifier _identifier) => new QualifiedType(_identifier);
        public static implicit operator QualifiedType(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator QualifiedType(string _type) => Parse(_type);
        public static implicit operator QualifiedType(Type _type) => FromType(_type);

        public QualifiedType(params SimpleType[] _path)
            : this((Namespace?) null, _path) { }

        public QualifiedType(Namespace? _namespace, params SimpleType[] _path)
            : this(_namespace, (IEnumerable<SimpleType>) _path) { }

        public QualifiedType(Identifier? _alias, params SimpleType[] _path)
            : this(_alias, (IEnumerable<SimpleType>) _path) { }

        public QualifiedType(IEnumerable<SimpleType> _path, bool _nullable, IEnumerable<int>? _ranks = null)
            : this((Namespace?) null, _path, _nullable, _ranks) { }

        public QualifiedType(Namespace? _namespace, IEnumerable<SimpleType> _path, bool _nullable = false, IEnumerable<int>? _ranks = null)
            : this(_namespace?.Alias, ((IEnumerable<Identifier>?) _namespace?.Path).EmptyIfNull().Select(_p => new SimpleType(_p)).Concat(_path), _nullable, _ranks) { }

        public QualifiedType(Identifier? _alias, IEnumerable<SimpleType> _path, bool _nullable = false, IEnumerable<int>? _ranks = null)
        {
            Alias = _alias;
            if (_path == null)
            {
                throw new ArgumentNullException(nameof(_path));
            }
            Path = _path.ToImmutableArray();
            if (!Path.Any())
            {
                throw new ArgumentException("Empty path", nameof(_path));
            }
            if (Path.AnyNull())
            {
                throw new ArgumentException("Null path item", nameof(_path));
            }
            IsNullable = _nullable;
            Ranks = _ranks.EmptyIfNull().ToImmutableArray();
            if (Ranks.Any(_i => _i < 1))
            {
                throw new ArgumentException("Non positive rank", nameof(_path));
            }
        }

        public Identifier? Alias { get; }
        public ImmutableArray<SimpleType> Path { get; }
        public bool IsNullable { get; }
        public ImmutableArray<int> Ranks { get; }

        public bool IsAliasQualified => Alias != null;
        public bool IsArray => Ranks.Any();
        public bool IsSimple => !IsQualified && !IsArray && !IsNullable;
        public bool IsQualified => IsAliasQualified || IsNamespaceQualified;
        public bool IsNamespaceQualified => Path.Length > 1;

        public SimpleType AsSimpleType()
        {
            if (!IsSimple)
            {
                throw new InvalidOperationException("Not a simple type");
            }
            return Path[1];
        }

        public override bool Equals(object? _obj) => Equals(_obj as QualifiedType);
        public bool Equals(QualifiedType? _other) => _other != null && Alias == _other.Alias && Path.SequenceEqual(_other.Path) && IsNullable == _other.IsNullable && Ranks.SequenceEqual(_other.Ranks);
        public override int GetHashCode() => Identity.CombineHash(Alias, Identity.CombineHashes(Path), IsNullable, Identity.CombineHashes(Ranks));
        public static bool operator ==(QualifiedType? _left, QualifiedType? _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(QualifiedType? _left, QualifiedType? _right) => !(_left == _right);
        public override string ToString()
            => $"{(IsAliasQualified ? $"{Alias}::" : "")}{string.Join('.', Path)}{(IsNullable ? "?" : "")}{string.Concat(Ranks.Select(_r => $"[{new string(',', _r - 1)}]"))}";

        public QualifiedType WithAlias(Identifier? _alias = null)
            => _alias == Alias ? this : new QualifiedType(_alias, Path, IsNullable, Ranks);

        public QualifiedType WithPath(params SimpleType[] _path)
            => WithPath((IEnumerable<SimpleType>) _path);

        public QualifiedType WithPath(IEnumerable<SimpleType> _path)
            => Path.SequenceEqual(_path) ? this : new QualifiedType(Alias, _path, IsNullable, Ranks);

        public QualifiedType WithIsNullable(bool _nullable)
            => _nullable == IsNullable ? this : new QualifiedType(Alias, Path, _nullable, Ranks);

        public QualifiedType WithRanks(int[] _ranks)
            => WithRanks((IEnumerable<int>) _ranks);

        public QualifiedType WithRanks(IEnumerable<int> _ranks)
            => Ranks.SequenceEqual(_ranks) ? this : new QualifiedType(Alias, Path, IsNullable, _ranks);

    }
}
