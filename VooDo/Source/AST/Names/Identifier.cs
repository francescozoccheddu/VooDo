

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compiling.Emission;
using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public sealed record Identifier : BodyNodeOrIdentifier
    {

        #region Creation

        public static Identifier Bool { get; } = "bool";
        public static Identifier Char { get; } = "char";
        public static Identifier String { get; } = "string";
        public static Identifier Byte { get; } = "byte";
        public static Identifier SByte { get; } = "sbyte";
        public static Identifier Short { get; } = "short";
        public static Identifier UShort { get; } = "ushort";
        public static Identifier Int { get; } = "int";
        public static Identifier UInt { get; } = "uint";
        public static Identifier Long { get; } = "long";
        public static Identifier ULong { get; } = "ulong";
        public static Identifier Decimal { get; } = "decimal";
        public static Identifier Float { get; } = "float";
        public static Identifier Double { get; } = "double";
        public static Identifier Object { get; } = "object";

        #endregion

        #region Conversion

        public static implicit operator Identifier(string _identifier) => new Identifier(_identifier);
        public static implicit operator string(Identifier _identifier) => _identifier.ToString();

        #endregion

        #region Members

        private readonly string m_identifier;

        public Identifier(string _identifier)
        {
            m_identifier = _identifier;
            if (!_identifier.All(_c => _c == '_' || char.IsLetterOrDigit(_c)))
            {
                throw new SyntaxError(this, "Non alphanumeric or underscore character").AsThrowable();
            }
            if (_identifier.Length == 0)
            {
                throw new SyntaxError(this, "Empty identifier").AsThrowable();
            }
            if (char.IsDigit(_identifier[0]))
            {
                throw new SyntaxError(this, "Non letter or undescore starting letter").AsThrowable();
            }
        }

        #endregion

        #region Overrides

        public override Identifier ReplaceNodes(Func<Node?, Node?> _map) => this;

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

        internal SyntaxToken? EmitPredefinedTypeKeywordToken(Tagger _tagger)
            => s_predefinedTypesTokens.TryGetValue(this, out SyntaxToken token)
            ? token.Own(_tagger, this) : null;
        internal override SyntaxNodeOrToken EmitNodeOrToken(Scope _scope, Tagger _tagger) => EmitToken(_tagger);
        internal SyntaxToken EmitToken(Tagger _tagger) => SyntaxFactory.Identifier(this).Own(_tagger, this);
        public override string ToString() => m_identifier;

        #endregion

    }

}
