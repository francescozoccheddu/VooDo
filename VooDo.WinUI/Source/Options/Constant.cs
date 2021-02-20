﻿
using VooDo.AST.Names;

namespace VooDo.WinUI.Options
{

    public sealed record Constant
    {

        public Constant(UnresolvedType _type, Identifier _name, object? _value)
        {
            Type = _type;
            Name = _name;
            Value = _value;
        }

        public UnresolvedType Type { get; init; }
        public Identifier Name { get; init; }
        public object? Value { get; init; }

    }

}
