

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.Language.Linking;

namespace VooDo.Language.AST.Names
{

    public sealed record Identifier : NodeOrIdentifier
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

        private static readonly ImmutableDictionary<string, SyntaxToken> s_predefinedTypesTokens =
            new SyntaxKind[] {
                SyntaxKind.BoolKeyword,
                SyntaxKind.CharKeyword,
                SyntaxKind.StringKeyword,
                SyntaxKind.ByteKeyword,
                SyntaxKind.SByteKeyword,
                SyntaxKind.ShortKeyword,
                SyntaxKind.UShortKeyword,
                SyntaxKind.IntKeyword,
                SyntaxKind.UIntKeyword,
                SyntaxKind.LongKeyword,
                SyntaxKind.ULongKeyword,
                SyntaxKind.DecimalKeyword,
                SyntaxKind.FloatKeyword,
                SyntaxKind.DoubleKeyword,
                SyntaxKind.ObjectKeyword
            }.Select(_k => SyntaxFactory.Token(_k))
            .ToImmutableDictionary(_t => _t.ValueText);

        internal SyntaxToken? EmitPredefinedTypeKeywordToken(Marker _marker)
            => s_predefinedTypesTokens.TryGetValue(this, out SyntaxToken token)
            ? token.Own(_marker, this) : null;
        internal override SyntaxNodeOrToken EmitNodeOrToken(Scope _scope, Marker _marker) => EmitToken(_marker);
        internal SyntaxToken EmitToken(Marker _marker) => SyntaxFactory.Identifier(this).Own(_marker, this);
        public override IEnumerable<Node> Children => Enumerable.Empty<Node>();
        public override string ToString() => m_identifier;

        #endregion

    }

}
