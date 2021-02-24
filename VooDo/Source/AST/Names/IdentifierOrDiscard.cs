
using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.AST.Names
{

    public sealed record IdentifierOrDiscard : Node
    {

        
        public static IdentifierOrDiscard Discard { get; } = new IdentifierOrDiscard(null);

        public static IdentifierOrDiscard Parse(string _name) => _name switch
        {
            "_" => Discard,
            _ => new IdentifierOrDiscard(new Identifier(_name))
        };

        public static IdentifierOrDiscard FromIdentifier(Identifier _identifier)
            => new IdentifierOrDiscard(_identifier);

        
        
        public static implicit operator IdentifierOrDiscard(string _type) => Parse(_type);
        public static implicit operator IdentifierOrDiscard(Identifier _identifier) => FromIdentifier(_identifier);
        public static implicit operator string(IdentifierOrDiscard _type) => _type.ToString();

        
        
        private IdentifierOrDiscard(Identifier? _identifier)
        {
            Identifier = _identifier;
        }

        public Identifier? Identifier { get; }
        public bool IsDiscard => Identifier is null;

        
        
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


        public override IEnumerable<Node> Children => IsDiscard ? Enumerable.Empty<Identifier>() : new[] { Identifier! };
        public override string ToString() => IsDiscard ? "_" : Identifier!.ToString();

        
    }

}
