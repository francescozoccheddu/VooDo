using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Factory
{

    public sealed class Identifier : IEquatable<Identifier>
    {

        public static Identifier FromSyntax(SyntaxToken _token) => _token.ValueText;

        public Identifier(string _identifier)
        {
            if (_identifier == null)
            {
                throw new ArgumentNullException(nameof(_identifier));
            }
            if (!_identifier.All(_c => _c == '_' || char.IsLetterOrDigit(_c)))
            {
                throw new ArgumentException("Non alphanumeric or underscore character", nameof(_identifier));
            }
            if (_identifier.Length == 0)
            {
                throw new ArgumentException("Empty identifier", nameof(_identifier));
            }
            if (char.IsDigit(_identifier[0]))
            {
                throw new ArgumentException("Non letter or undescore starting letter", nameof(_identifier));
            }
            m_identifier = _identifier;
        }

        private readonly string m_identifier;

        public static implicit operator string(Identifier _identifier) => _identifier.m_identifier;
        public static implicit operator Identifier(string _identifier) => new Identifier(_identifier);

        public override bool Equals(object _obj) => Equals(_obj as Identifier);
        public bool Equals(Identifier _other) => _other != null && m_identifier == _other.m_identifier;
        public static bool operator ==(Identifier _left, Identifier _right) => EqualityComparer<Identifier>.Default.Equals(_left, _right);
        public static bool operator !=(Identifier _left, Identifier _right) => !(_left == _right);
        public override int GetHashCode() => m_identifier.GetHashCode();
        public override string ToString() => this;

    }

}
