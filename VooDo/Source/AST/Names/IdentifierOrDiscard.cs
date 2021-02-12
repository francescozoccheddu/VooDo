using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Compilation;
using VooDo.Compilation;

namespace VooDo.AST.Names
{

    public sealed record IdentifierOrDiscard : Node
    {

        #region Creation

        public static IdentifierOrDiscard Discard { get; } = new IdentifierOrDiscard(null);

        public static IdentifierOrDiscard Parse(string _name) => _name switch
        {
            "_" => Discard,
            _ => new IdentifierOrDiscard(new Identifier(_name))
        };

        public static IdentifierOrDiscard FromIdentifier(Identifier _identifier)
            => new IdentifierOrDiscard(_identifier);

        #endregion

        #region Conversion

        public static implicit operator IdentifierOrDiscard(string _type) => Parse(_type);
        public static implicit operator IdentifierOrDiscard(Identifier _identifier) => FromIdentifier(_identifier);
        public static implicit operator string(IdentifierOrDiscard _type) => _type.ToString();

        #endregion

        #region Members

        private IdentifierOrDiscard(Identifier? _identifier)
        {
            Identifier = _identifier;
        }

        public Identifier? Identifier { get; }
        public bool IsDiscard => Identifier is null;

        #endregion

        #region Overrides

        internal override VariableDesignationSyntax EmitNode(Scope _scope, Marker _marker)
            => (IsDiscard
            ? (VariableDesignationSyntax) SyntaxFactory.DiscardDesignation()
            : SyntaxFactory.SingleVariableDesignation(Identifier!.EmitToken(_marker)))
            .Own(_marker, this);
        public override IEnumerable<Identifier> Children => IsDiscard ? Enumerable.Empty<Identifier>() : new[] { Identifier! };
        public override string ToString() => IsDiscard ? "_" : Identifier!.ToString();

        #endregion

    }

}
