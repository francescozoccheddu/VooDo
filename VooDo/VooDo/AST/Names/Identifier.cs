

using Microsoft.CodeAnalysis;

using System;
using System.Linq;

using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Names
{

    public sealed record Identifier : Node
    {

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

        public static implicit operator Identifier(string _identifier) => _identifier is null ? null! : new Identifier(_identifier);
        public static implicit operator string(Identifier _identifier) => _identifier?.ToString()!;

        private readonly string m_identifier;

        public Identifier(string _identifier)
        {
            m_identifier = _identifier;
            if (_identifier.StartsWith(Identifiers.reservedPrefix))
            {
                throw new SyntaxError(this, $"'{Identifiers.reservedPrefix}' is a reserved prefix").AsThrowable();
            }
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

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map) => this;

        public override string ToString() => m_identifier;

    }

}
