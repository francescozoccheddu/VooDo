
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Factory
{
    public sealed class Namespace : IReadOnlyList<Identifier>, IEquatable<Namespace>
    {

        public static Namespace FromSyntax(IdentifierNameSyntax _name)
            => Identifier.FromSyntax(_name);

        public static Namespace FromSyntax(QualifiedNameSyntax _name)
        {
            if (_name == null)
            {
                throw new ArgumentNullException(nameof(_name));
            }
            List<Identifier> path = new List<Identifier>();
            QualifiedNameSyntax qualifiedName = _name;
            while (true)
            {
                if (qualifiedName.Right is IdentifierNameSyntax rightName)
                {
                    path.Add(Identifier.FromSyntax(rightName));
                }
                else
                {
                    throw new ArgumentException("Non identifier element", nameof(_name));
                }
                if (qualifiedName.Left is QualifiedNameSyntax leftQualifiedName)
                {
                    qualifiedName = leftQualifiedName;
                }
                else if (qualifiedName.Left is IdentifierNameSyntax leftName)
                {
                    path.Add(Identifier.FromSyntax(leftName));
                    break;
                }
                else
                {
                    throw new ArgumentException("Non identifier element", nameof(_name));
                }
            }
            path.Reverse();
            return path;
        }

        public static Namespace Parse(string _namespace)
        {
            if (_namespace == null)
            {
                throw new ArgumentNullException(_namespace);
            }
            return new Namespace(_namespace.Split('.').Select(_i => new Identifier(_i)));
        }

        public static implicit operator Namespace(string _namespace) => Parse(_namespace);
        public static implicit operator Namespace(Identifier _identifier) => new Namespace(_identifier);
        public static implicit operator Namespace(Identifier[] _path) => new Namespace(_path);
        public static implicit operator Namespace(List<Identifier> _path) => new Namespace(_path);
        public static implicit operator Namespace(ImmutableArray<Identifier> _path) => new Namespace(_path);

        public Namespace(params Identifier[] _path)
            : this((IEnumerable<Identifier>) _path) { }

        public Namespace(IEnumerable<Identifier> _path)
        {
            if (_path == null)
            {
                throw new ArgumentNullException(nameof(_path));
            }
            m_path = _path.ToImmutableArray();
            if (m_path.IsEmpty)
            {
                throw new ArgumentException("Empty path", nameof(_path));
            }
            if (m_path.AnyNull())
            {
                throw new ArgumentException("Null identifier", nameof(_path));
            }
        }

        private readonly ImmutableArray<Identifier> m_path;

        public int Count => ((IReadOnlyCollection<Identifier>) m_path).Count;
        public Identifier this[int _index] => ((IReadOnlyList<Identifier>) m_path)[_index];
        public IEnumerator<Identifier> GetEnumerator() => ((IEnumerable<Identifier>) m_path).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) m_path).GetEnumerator();

        public override bool Equals(object _obj) => Equals(_obj as Namespace);
        public bool Equals(Namespace _other) => _other != null && m_path.SequenceEqual(_other.m_path);
        public static bool operator ==(Namespace _left, Namespace _right) => EqualityComparer<Namespace>.Default.Equals(_left, _right);
        public static bool operator !=(Namespace _left, Namespace _right) => !(_left == _right);
        public override int GetHashCode() => Identity.CombineHashes(m_path);
        public override string ToString() => string.Join('.', m_path);

    }
}
