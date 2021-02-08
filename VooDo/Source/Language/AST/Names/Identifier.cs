

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Language.AST.Names
{

    public sealed record Identifier : Node
    {

        #region Creation

        public static Identifier FromSyntax(SyntaxToken _token) => new Identifier(_token.ValueText);

        #endregion

        #region Conversion

        public static implicit operator Identifier(string _identifier) => new Identifier(_identifier);
        public static implicit operator string(Identifier _identifier) => _identifier.ToString();

        #endregion

        #region Members

        private readonly string m_identifier;

        public Identifier(string _identifier)
        {
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

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => Enumerable.Empty<Node>();
        public override string ToString() => m_identifier;

        #endregion

    }

}
