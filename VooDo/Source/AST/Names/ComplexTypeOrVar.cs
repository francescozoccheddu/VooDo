
using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Parsing;

namespace VooDo.AST.Names
{

    public sealed record ComplexTypeOrVar : BodyNode
    {

        
        public static ComplexTypeOrVar Var { get; } = new ComplexTypeOrVar(null);

        public static ComplexTypeOrVar Parse(string _type)
            => Parser.ComplexTypeOrVar(_type);

        public static ComplexTypeOrVar FromType(Type _type, bool _ignoreUnbound = false)
            => ComplexType.FromType(_type, _ignoreUnbound);

        public static ComplexTypeOrVar FromType<TType>()
            => FromType(typeof(TType));

        public static ComplexTypeOrVar FromComplexType(ComplexType _type)
            => new ComplexTypeOrVar(_type);

        
        
        public static implicit operator ComplexTypeOrVar(string _type) => Parse(_type);
        public static implicit operator ComplexTypeOrVar(Type _type) => FromType(_type);
        public static implicit operator ComplexTypeOrVar(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator ComplexTypeOrVar(Identifier _identifier) => new QualifiedType(_identifier);
        public static implicit operator ComplexTypeOrVar(ComplexType _complexType) => FromComplexType(_complexType);
        public static implicit operator string(ComplexTypeOrVar _type) => _type.ToString();

        
        
        private ComplexTypeOrVar(ComplexType? _type)
        {
            Type = _type;
        }

        public ComplexType? Type { get; }
        public bool IsVar => Type is null;

        
        
        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            if (IsVar)
            {
                return this;
            }
            ComplexType? newType = (ComplexType?) _map(Type);
            if (ReferenceEquals(newType, Type))
            {
                return this;
            }
            else
            {
                return new ComplexTypeOrVar(newType);
            }
        }


        public override IEnumerable<Node> Children => IsVar ? Enumerable.Empty<ComplexType>() : new[] { Type! };
        public override string ToString() => IsVar ? "var" : Type!.ToString();

        
    }

}
