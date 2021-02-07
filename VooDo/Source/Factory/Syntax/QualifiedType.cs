

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Factory.Syntax
{
    public sealed class QualifiedType : ComplexType, IEquatable<QualifiedType>
    {

        public static new QualifiedType FromSyntax(TypeSyntax _type, bool _ignoreUnbound = false)
            => (Unwrap(_type, out bool nullable, out ImmutableArray<int> ranks) switch
            {
                QualifiedNameSyntax qualified => FromSyntax(qualified, _ignoreUnbound),
                AliasQualifiedNameSyntax aliased => FromSyntax(aliased, _ignoreUnbound),
                SimpleNameSyntax simple => FromSyntax(simple),
                PredefinedTypeSyntax predefined => FromSyntax(predefined, _ignoreUnbound),
                _ => throw new ArgumentException("Not a qualified type", nameof(_type)),
            }).WithIsNullable(nullable).WithRanks(ranks);

        public static QualifiedType FromSyntax(SimpleNameSyntax _type, bool _ignoreUnbound = false)
            => SimpleType.FromSyntax(_type, _ignoreUnbound);

        public static QualifiedType FromSyntax(QualifiedNameSyntax _type, bool _ignoreUnbound = false)
        {
            QualifiedType left = FromSyntax(_type.Left);
            return left.WithPath(left.Path.Add(SimpleType.FromSyntax(_type.Right, _ignoreUnbound)));
        }

        public static QualifiedType FromSyntax(AliasQualifiedNameSyntax _type, bool _ignoreUnbound = false)
            => new QualifiedType(Identifier.FromSyntax(_type.Alias.Identifier), SimpleType.FromSyntax(_type.Name, _ignoreUnbound));

        public static QualifiedType FromSyntax(PredefinedTypeSyntax _type)
            => SimpleType.FromSyntax(_type);

        public static new QualifiedType Parse(string _type, bool _ignoreUnbound = false)
            => FromSyntax(SyntaxFactory.ParseTypeName(_type), _ignoreUnbound);

        public static new QualifiedType FromType(Type _type, bool _ignoreUnbound = false)
        {
            if (_type.IsGenericTypeDefinition && !_ignoreUnbound)
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
                return new QualifiedType(SimpleType.FromType(_type, _ignoreUnbound));
            }
            else
            {
                Type type = Unwrap(_type, out bool nullable, out ImmutableArray<int> ranks);
                List<SimpleType> path = new List<SimpleType>
                {
                    type
                };
                while (type.IsNested)
                {
                    type = type.DeclaringType!;
                    path.Add(SimpleType.FromType(type, _ignoreUnbound));
                }
                path.Reverse();
                ranks.Reverse();
                Namespace? @namespace = type.Namespace != null ? Namespace.Parse(type.Namespace) : null;
                return new QualifiedType(@namespace, path, nullable, ranks);
            }
        }

        public static new QualifiedType FromType<TType>()
            => FromType(typeof(TType), false);

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

        public QualifiedType(Identifier? _alias, IEnumerable<SimpleType> _path, bool _nullable = false, IEnumerable<int>? _ranks = null) : base(_nullable, _ranks)
        {
            Alias = _alias;
            if (_path is null)
            {
                throw new ArgumentNullException(nameof(_path));
            }
            Path = _path.ToImmutableArray();
            if (Path.IsEmpty)
            {
                throw new ArgumentException("Empty path", nameof(_path));
            }

        }

        public Identifier? Alias { get; }
        public ImmutableArray<SimpleType> Path { get; }

        public bool IsAliasQualified => Alias is not null;

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
        public bool Equals(QualifiedType? _other) => _other is not null && Alias == _other.Alias && Path.SequenceEqual(_other.Path) && base.Equals(_other);
        public override int GetHashCode() => Identity.CombineHash(Alias, Identity.CombineHashes(Path), base.GetHashCode());
        public static bool operator ==(QualifiedType? _left, QualifiedType? _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(QualifiedType? _left, QualifiedType? _right) => !(_left == _right);
        public override string ToString()
            => $"{(IsAliasQualified ? $"{Alias}::" : "")}{string.Join('.', Path)}{base.ToString()}";

        public QualifiedType WithAlias(Identifier? _alias = null)
            => _alias == Alias ? this : new QualifiedType(_alias, Path, IsNullable, Ranks);

        public QualifiedType WithPath(params SimpleType[] _path)
            => WithPath(_path);

        public QualifiedType WithPath(IEnumerable<SimpleType> _path)
            => Path.SequenceEqual(_path) ? this : new QualifiedType(Alias, _path, IsNullable, Ranks);

        public override QualifiedType WithIsNullable(bool _nullable)
            => _nullable == IsNullable ? this : new QualifiedType(Alias, Path, _nullable, Ranks);

        public override QualifiedType WithRanks(IEnumerable<int> _ranks)
            => Ranks.SequenceEqual(_ranks) ? this : new QualifiedType(Alias, Path, IsNullable, _ranks);

    }
}
