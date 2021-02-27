
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Parsing;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public sealed record QualifiedType : ComplexType
    {

        public static new QualifiedType Parse(string _type)
            => Parser.QualifiedType(_type);

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
                Type type = Unwrap(_type, out bool nullable, out ImmutableArray<RankSpecifier> ranks);
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



        public static implicit operator QualifiedType(string _type) => Parse(_type);
        public static implicit operator QualifiedType(Type _type) => FromType(_type);
        public static implicit operator QualifiedType(Identifier _name) => new QualifiedType(new SimpleType(_name));
        public static implicit operator QualifiedType(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator string(QualifiedType _complexType) => _complexType.ToString();



        public QualifiedType(params SimpleType[] _path) : this(null, (IEnumerable<SimpleType>)_path) { }
        public QualifiedType(Namespace? _namespace, params SimpleType[] _path) : this(_namespace, (IEnumerable<SimpleType>)_path) { }
        public QualifiedType(Identifier? _alias, params SimpleType[] _path) : this(_alias, (IEnumerable<SimpleType>)_path) { }
        public QualifiedType(IEnumerable<SimpleType> _path) : this(null, _path) { }
        public QualifiedType(Namespace? _namespace, IEnumerable<SimpleType> _path)
            : this(_namespace?.Alias,
                  ((IEnumerable<Identifier>?)_namespace?.Path)
                  .EmptyIfNull()
                  .Select(_i => new SimpleType(_i))
                  .Concat(_path))
        { }
        public QualifiedType(Identifier? _alias, IEnumerable<SimpleType> _path) : this(_alias, _path.ToImmutableArray()) { }



        public QualifiedType(Identifier? _alias, ImmutableArray<SimpleType> _path)
        {
            Alias = _alias;
            Path = _path;
        }

        public Identifier? Alias { get; init; }

        private ImmutableArray<SimpleType> m_path;
        public ImmutableArray<SimpleType> Path
        {
            get => m_path;
            init
            {
                if (value.IsDefaultOrEmpty)
                {
                    throw new SyntaxError(this, "QualifiedType path cannot be empty").AsThrowable();
                }
                m_path = value;
            }
        }
        public bool IsAliasQualified => Alias is not null;
        public bool IsSimple => !IsQualified && !IsArray && !IsNullable;
        public bool IsQualified => IsAliasQualified || IsNamespaceQualified;
        public bool IsNamespaceQualified => Path.Length > 1;
        public SimpleType SimpleType => Path[0];



        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Identifier? newAlias = (Identifier?)_map(Alias);
            ImmutableArray<SimpleType> newPath = Path.Map(_map).NonNull();
            QualifiedType newThis = (QualifiedType)base.ReplaceNodes(_map);
            if (ReferenceEquals(newAlias, Alias) && newPath == Path)
            {
                return newThis;
            }
            else
            {
                return newThis with
                {
                    Alias = newAlias,
                    Path = newPath
                };
            }
        }



        public override IEnumerable<Node> Children =>
            (IsAliasQualified ? new Node[] { Alias! } : Enumerable.Empty<Node>())
            .Concat(Path);
        public override string ToString() => (IsAliasQualified ? $"{Alias}::" : "") + string.Join(".", Path) + base.ToString();


    }
}
