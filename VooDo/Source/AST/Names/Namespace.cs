using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.Errors.Problems;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public sealed record Namespace : Node
    {

        #region Creation

        public static Namespace FromSyntax(AliasQualifiedNameSyntax _syntax)
            => new Namespace(Identifier.FromSyntax(_syntax.Alias.Identifier), ImmutableArray.Create(Identifier.FromSyntax(_syntax.Name.Identifier)));

        public static Namespace FromSyntax(IdentifierNameSyntax _syntax)
            => new Namespace(null, ImmutableArray.Create(Identifier.FromSyntax(_syntax.Identifier)));

        public static Namespace FromSyntax(QualifiedNameSyntax _syntax)
        {
            if (_syntax.Right is IdentifierNameSyntax name)
            {
                Namespace left = FromSyntax(_syntax.Left);
                return left with
                {
                    Path = left.Path.Add(Identifier.FromSyntax(name.Identifier))
                };
            }
            else
            {
                throw new ArgumentException("Not a namespace type", nameof(_syntax));
            }
        }

        public static Namespace FromSyntax(TypeSyntax _syntax) => _syntax switch
        {
            IdentifierNameSyntax name => FromSyntax(name),
            QualifiedNameSyntax qualified => FromSyntax(qualified),
            AliasQualifiedNameSyntax aliased => FromSyntax(aliased),
            _ => throw new ArgumentException("Not a namespace type", nameof(_syntax))
        };

        public static Namespace Parse(string _namespace)
            => FromSyntax(SyntaxFactory.ParseTypeName(_namespace));

        #endregion

        #region Conversion

        public static implicit operator Namespace(string _namespace) => Parse(_namespace);
        public static implicit operator Namespace(Identifier _path) => new Namespace(new[] { _path });
        public static implicit operator Namespace(Identifier[] _path) => new Namespace(_path);
        public static implicit operator Namespace(List<Identifier> _path) => new Namespace(_path.ToImmutableArray());
        public static implicit operator Namespace(ImmutableArray<Identifier> _path) => new Namespace(_path);
        public static implicit operator string(Namespace _namespace) => _namespace.ToString();

        #endregion

        #region Additional contructors

        public Namespace(params Identifier[] _path) : this(null, _path.ToImmutableArray()) { }
        public Namespace(Identifier? _alias, params Identifier[] _path) : this(_alias, _path.ToImmutableArray()) { }
        public Namespace(IEnumerable<Identifier> _path) : this(null, _path.ToImmutableArray()) { }
        public Namespace(Identifier? _alias, IEnumerable<Identifier> _path) : this(_alias, _path.ToImmutableArray()) { }
        public Namespace(ImmutableArray<Identifier> _path) : this(null, _path) { }

        #endregion

        #region Members

        public Namespace(Identifier? _alias, ImmutableArray<Identifier> _path)
        {
            Alias = _alias;
            Path = _path;
        }

        public Identifier? Alias { get; init; }

        private ImmutableArray<Identifier> m_path;
        public ImmutableArray<Identifier> Path
        {
            get => m_path;
            init
            {
                if (value.IsDefaultOrEmpty)
                {
                    throw new SyntaxError(this, "Namespace path cannot be empty").AsThrowable();
                }
                m_path = value;
            }

        }
        public bool IsAliasQualified => Alias is not null;

        #endregion

        #region Overrides

        public override Namespace ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            Identifier? newAlias = (Identifier?) _map(Alias);
            ImmutableArray<Identifier> newPath = Path.Map(_map).NonNull();
            if (ReferenceEquals(newAlias, Alias) && newPath == Path)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Alias = newAlias,
                    Path = newPath
                };
            }
        }

        internal override NameSyntax EmitNode(Scope _scope, Marker _marker)
        {
            IdentifierNameSyntax[] path = Path.Select(_i => SyntaxFactory.IdentifierName(_i.EmitToken(_marker)).Own(_marker, _i)).ToArray();
            NameSyntax type = path[0];
            if (IsAliasQualified)
            {
                type = SyntaxFactory.AliasQualifiedName(
                    SyntaxFactory.IdentifierName(Alias!.EmitToken(_marker)),
                    (SimpleNameSyntax) type);
            }
            foreach (SimpleType name in Path.Skip(1))
            {
                type = SyntaxFactory.QualifiedName(type, name.EmitNode(_scope, _marker));
            }
            return type.Own(_marker, this);
        }

        public override IEnumerable<NodeOrIdentifier> Children => (IsAliasQualified ? new NodeOrIdentifier[] { Alias! } : Enumerable.Empty<NodeOrIdentifier>()).Concat(Path);
        public override string ToString() => (IsAliasQualified ? $"{Alias}::" : "") + string.Join('.', Path);

        #endregion

    }
}
