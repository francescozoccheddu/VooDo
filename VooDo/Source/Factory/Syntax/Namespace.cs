
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Factory
{
    public sealed class Namespace : IEquatable<Namespace>
    {

        public static Namespace FromSyntax(AliasQualifiedNameSyntax _syntax)
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            return new Namespace(Identifier.FromSyntax(_syntax.Alias.Identifier), new Identifier[] { Identifier.FromSyntax(_syntax.Name.Identifier) });
        }

        public static Namespace FromSyntax(IdentifierNameSyntax _syntax)
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            return new Namespace(null, new Identifier[] { Identifier.FromSyntax(_syntax.Identifier) });
        }

        public static Namespace FromSyntax(QualifiedNameSyntax _syntax)
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            if (_syntax.Right is IdentifierNameSyntax name)
            {
                Namespace left = FromSyntax(_syntax.Left);
                return left.WithPath(left.Path.Add(Identifier.FromSyntax(name.Identifier)));
            }
            else
            {
                throw new ArgumentException("Not a namespace type", nameof(_syntax));
            }
        }

        public static Namespace FromSyntax(TypeSyntax _syntax)
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            if (_syntax is IdentifierNameSyntax name)
            {
                return FromSyntax(name);
            }
            else if (_syntax is QualifiedNameSyntax qualified)
            {
                return FromSyntax(qualified);
            }
            else if (_syntax is AliasQualifiedNameSyntax aliased)
            {
                return FromSyntax(aliased);
            }
            else
            {
                throw new ArgumentException("Not a namespace type", nameof(_syntax));
            }
        }

        public static Namespace Parse(string _namespace)
        {
            if (_namespace == null)
            {
                throw new ArgumentNullException(_namespace);
            }
            return FromSyntax(SyntaxFactory.ParseTypeName(_namespace));
        }

        public static implicit operator Namespace(string _namespace) => Parse(_namespace);
        public static implicit operator Namespace(Identifier _path) => new Namespace(new[] { _path });
        public static implicit operator Namespace(Identifier[] _path) => new Namespace(_path);
        public static implicit operator Namespace(List<Identifier> _path) => new Namespace(_path);
        public static implicit operator Namespace(ImmutableArray<Identifier> _path) => new Namespace(_path);

        public Namespace(params Identifier[] _path)
            : this(null, _path) { }

        public Namespace(IEnumerable<Identifier> _path)
            : this(null, _path) { }

        public Namespace(Identifier _alias, IEnumerable<Identifier> _path)
        {
            if (_path == null)
            {
                throw new ArgumentNullException(nameof(_path));
            }
            Path = _path.ToImmutableArray();
            Alias = _alias;
            if (Path.AnyNull())
            {
                throw new ArgumentException("Null identifier", nameof(_path));
            }
            if (Path.IsEmpty)
            {
                throw new ArgumentException("Empty path", nameof(_path));
            }
        }

        public Identifier Alias { get; }
        public ImmutableArray<Identifier> Path { get; }
        public bool IsAliasQualified => Alias != null;

        public override bool Equals(object _obj) => Equals(_obj as Namespace);
        public bool Equals(Namespace _other) => _other != null && Alias == _other.Alias && Path.SequenceEqual(_other.Path);
        public static bool operator ==(Namespace _left, Namespace _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(Namespace _left, Namespace _right) => !(_left == _right);
        public override int GetHashCode() => Identity.CombineHash(Alias, Identity.CombineHashes(Path));
        public override string ToString() => (IsAliasQualified ? $"{Alias}::" : "") + string.Join('.', Path);

        public Namespace WithAlias(Identifier _alias)
            => new Namespace(_alias, Path);

        public Namespace WithPath(IEnumerable<Identifier> _path)
            => new Namespace(Alias, _path);

    }
}
