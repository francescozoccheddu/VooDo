

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Language.AST.Names
{

    public sealed record QualifiedType(Identifier? Alias, ImmutableArray<SimpleType> Path) : ComplexType
    {

        #region Creation

        public static new QualifiedType FromSyntax(TypeSyntax _type, bool _ignoreUnbound = false)
        {
            QualifiedType type = Unwrap(_type, out bool nullable, out ImmutableArray<int> ranks) switch
            {
                QualifiedNameSyntax qualified => FromSyntax(qualified, _ignoreUnbound),
                AliasQualifiedNameSyntax aliased => FromSyntax(aliased, _ignoreUnbound),
                SimpleNameSyntax simple => FromSyntax(simple),
                PredefinedTypeSyntax predefined => FromSyntax(predefined),
                _ => throw new ArgumentException("Not a qualified type", nameof(_type)),
            };
            return type with
            {
                IsNullable = nullable,
                Ranks = ranks
            };
        }

        public static QualifiedType FromSyntax(SimpleNameSyntax _type, bool _ignoreUnbound = false)
            => SimpleType.FromSyntax(_type, _ignoreUnbound);

        public static QualifiedType FromSyntax(QualifiedNameSyntax _type, bool _ignoreUnbound = false)
        {
            QualifiedType left = FromSyntax(_type.Left);
            return left with
            {
                Path = left.Path.Add(SimpleType.FromSyntax(_type.Right, _ignoreUnbound))
            };
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
                Namespace? @namespace = type.Namespace != null ? Namespace.Parse(type.Namespace) : null;
                return new QualifiedType(@namespace, path) with
                {
                    IsNullable = nullable,
                    Ranks = ranks.Reverse().ToImmutableArray()
                };
            }
        }

        public static new QualifiedType FromType<TType>()
            => FromType(typeof(TType), false);

        #endregion

        #region Conversion

        public static implicit operator QualifiedType(string _type) => Parse(_type);
        public static implicit operator QualifiedType(Type _type) => FromType(_type);
        public static implicit operator QualifiedType(Identifier _name) => new QualifiedType(new SimpleType(_name));
        public static implicit operator QualifiedType(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator string(QualifiedType _complexType) => _complexType.ToString();

        #endregion

        #region Delegating constructors

        public QualifiedType(params SimpleType[] _path) : this(null, (IEnumerable<SimpleType>) _path) { }
        public QualifiedType(Namespace? _namespace, params SimpleType[] _path) : this(_namespace, (IEnumerable<SimpleType>) _path) { }
        public QualifiedType(Identifier? _alias, params SimpleType[] _path) : this(_alias, (IEnumerable<SimpleType>) _path) { }
        public QualifiedType(IEnumerable<SimpleType> _path) : this(null, _path) { }
        public QualifiedType(Namespace? _namespace, IEnumerable<SimpleType> _path)
            : this(_namespace?.Alias,
                  ((IEnumerable<Identifier>?) _namespace?.Path)
                  .EmptyIfNull()
                  .Select(_i => new SimpleType(_i))
                  .Concat(_path))
        { }
        public QualifiedType(Identifier? _alias, IEnumerable<SimpleType> _path) : this(_alias, _path.ToImmutableArray()) { }

        #endregion

        #region Members

        private ImmutableArray<SimpleType> m_path = Path.NonEmpty();
        public ImmutableArray<SimpleType> Path
        {
            get => m_path;
            init => m_path.NonEmpty();
        }
        public bool IsAliasQualified => Alias is not null;
        public bool IsSimple => !IsQualified && !IsArray && !IsNullable;
        public bool IsQualified => IsAliasQualified || IsNamespaceQualified;
        public bool IsNamespaceQualified => Path.Length > 1;
        public SimpleType SimpleType => Path[0];

        #endregion

        #region Overrides

        public override string ToString() => (IsAliasQualified ? $"{Alias}::" : "") + string.Join('.', Path) + base.ToString();

        #endregion

    }
}
