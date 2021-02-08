namespace VooDo.Language.AST.Names
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

        public override string ToString() => IsDiscard ? "_" : Identifier!.ToString();

        #endregion

    }

}
