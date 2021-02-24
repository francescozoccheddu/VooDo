
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.AST.Names
{

    public sealed record SimpleType : Node
    {

        
        private static readonly Dictionary<Type, string> s_typenames =
                new Dictionary<Type, string>()
            {
                { typeof(byte), "byte" },
                { typeof(sbyte), "sbyte" },
                { typeof(short), "short" },
                { typeof(ushort), "ushort" },
                { typeof(int), "int" },
                { typeof(uint), "uint" },
                { typeof(long), "long" },
                { typeof(ulong), "ulong" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(decimal), "decimal" },
                { typeof(object), "object" },
                { typeof(bool), "bool" },
                { typeof(char), "char" },
                { typeof(string), "string" },
            };

        public static SimpleType Parse(string _type)
            => SimpleType.Parse(_type);

        public static SimpleType FromType<TType>(bool _ignoreUnboundGenerics = false)
            => FromType(typeof(TType), _ignoreUnboundGenerics);

        public static SimpleType FromType(Type _type, bool _ignoreUnboundGenerics = false)
        {
            if (_type.IsGenericTypeDefinition && !_ignoreUnboundGenerics)
            {
                throw new ArgumentException("Unbound type", nameof(_type));
            }
            if (_type.IsGenericParameter)
            {
                throw new ArgumentException("Generic parameter type", nameof(_type));
            }
            if (_type.IsPointer)
            {
                throw new ArgumentException("Pointer type", nameof(_type));
            }
            if (_type.IsArray)
            {
                throw new ArgumentException("Array type", nameof(_type));
            }
            if (_type.IsByRef)
            {
                throw new ArgumentException("Ref type", nameof(_type));
            }
            if (_type == typeof(void))
            {
                throw new ArgumentException("Void type", nameof(_type));
            }
            if (_type.IsPrimitive)
            {
                return new SimpleType(new Identifier(s_typenames[_type]));
            }
            else
            {
                string? name = _type.Name;
                int length = _type.Name.IndexOf('`');
                if (length > 0)
                {
                    name = _type.Name.Substring(0, length);
                }
                return new SimpleType(
                    new Identifier(name),
                    _type.GenericTypeArguments.Select(_a => ComplexType.FromType(_a, _ignoreUnboundGenerics)).ToImmutableArray());
            }
        }

        
        
        public static implicit operator SimpleType(string _type) => Parse(_type);
        public static implicit operator SimpleType(Identifier _name) => new SimpleType(_name);
        public static implicit operator SimpleType(Type _type) => FromType(_type);
        public static implicit operator string(SimpleType _simpleType) => _simpleType.ToString();

        
        
        public SimpleType(Identifier _name, params ComplexType[] _typeArguments) : this(_name, _typeArguments.ToImmutableArray()) { }
        public SimpleType(Identifier _name, IEnumerable<ComplexType>? _typeArguments) : this(_name, _typeArguments.EmptyIfNull().ToImmutableArray()) { }

        
        
        public SimpleType(Identifier _name, ImmutableArray<ComplexType> _typeArguments = default)
        {
            Name = _name;
            TypeArguments = _typeArguments;
        }

        public Identifier Name { get; init; }

        private ImmutableArray<ComplexType> m_typeArguments;
        public ImmutableArray<ComplexType> TypeArguments
        {
            get => m_typeArguments;
            init => m_typeArguments = value.EmptyIfDefault();
        }
        public bool IsGeneric => !TypeArguments.IsEmpty;

        
        

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Identifier newName = (Identifier) _map(Name).NonNull();
            ImmutableArray<ComplexType> newTypeArguments = TypeArguments.Map(_map).NonNull();
            if (ReferenceEquals(newName, Name) && newTypeArguments == TypeArguments)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Name = newName,
                    TypeArguments = newTypeArguments
                };
            }
        }

        public override IEnumerable<Node> Children => new Node[] { Name }.Concat(TypeArguments);
        public override string ToString() => IsGeneric ? $"{Name}<{string.Join(", ", TypeArguments)}>" : $"{Name}";

        
    }

}



