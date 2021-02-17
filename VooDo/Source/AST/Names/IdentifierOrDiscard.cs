using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Compiling.Emission;

namespace VooDo.AST.Names
{

    public sealed record IdentifierOrDiscard : BodyNode
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

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            if (IsDiscard)
            {
                return this;
            }
            Identifier? newIdentifier = (Identifier?) _map(Identifier);
            if (ReferenceEquals(newIdentifier, Identifier))
            {
                return this;
            }
            else
            {
                return new IdentifierOrDiscard(newIdentifier);
            }
        }

        internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
            => (IsDiscard
            ? (VariableDesignationSyntax) SyntaxFactory.DiscardDesignation()
            : SyntaxFactory.SingleVariableDesignation(Identifier!.EmitToken(_tagger)))
            .Own(_tagger, this);
        public override IEnumerable<Node> Children => IsDiscard ? Enumerable.Empty<Identifier>() : new[] { Identifier! };
        public override string ToString() => IsDiscard ? "_" : Identifier!.ToString();

        #endregion

    }

}
