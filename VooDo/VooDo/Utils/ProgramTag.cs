
using VooDo.AST.Expressions;
using VooDo.AST.Names;

namespace VooDo.Utils
{

    public sealed record ProgramTag
    {

        public ProgramTag(Identifier _name, bool _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, int _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, uint _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, short _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, ushort _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, long _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, ulong _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, decimal _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, sbyte _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, byte _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, char _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, string? _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, float _value) : this(_name, new LiteralExpression(_value)) { }
        public ProgramTag(Identifier _name, double _value) : this(_name, new LiteralExpression(_value)) { }

        public ProgramTag(Identifier _name, LiteralExpression _value)
            : this(_value.Value switch
            {
                null => "object",
                bool => "bool",
                int => "int",
                uint => "uint",
                short => "short",
                ushort => "ushort",
                long => "long",
                ulong => "ulong",
                decimal => "decimal",
                sbyte => "sbyte",
                byte => "byte",
                char => "char",
                string => "string",
                float => "float",
                double => "double"
            }, _name, _value)
        { }

        public ProgramTag(ComplexType _type, Identifier _name, Expression _value)
        {
            Type = _type;
            Name = _name;
            Value = _value;
        }

        public ComplexType Type { get; init; }
        public Identifier Name { get; init; }
        public Expression Value { get; init; }

    }

}
