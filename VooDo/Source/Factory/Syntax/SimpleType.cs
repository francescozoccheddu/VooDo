

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Utils;

namespace VooDo.Factory.Syntax
{

    public sealed class SimpleType : IEquatable<SimpleType>
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

        public static SimpleType FromSyntax(TypeSyntax _syntax, bool _ignoreUnboundGenerics = false) => _syntax switch
        {
            SimpleNameSyntax simple => FromSyntax(simple, _ignoreUnboundGenerics),
            PredefinedTypeSyntax predefined => FromSyntax(predefined),
            _ => throw new ArgumentException("Not a simple type", nameof(_syntax)),
        };

        public static SimpleType FromSyntax(PredefinedTypeSyntax _syntax)
            => new SimpleType(Identifier.FromSyntax(_syntax.Keyword));

        public static SimpleType FromSyntax(SimpleNameSyntax _syntax, bool _ignoreUnboundGenerics = false) => _syntax switch
        {
            IdentifierNameSyntax name
                => new SimpleType(Identifier.FromSyntax(name.Identifier)),
            GenericNameSyntax genericName when genericName.IsUnboundGenericName && _ignoreUnboundGenerics
                => new SimpleType(Identifier.FromSyntax(genericName.Identifier)),
            GenericNameSyntax genericName when !genericName.IsUnboundGenericName
                => new SimpleType(
                    Identifier.FromSyntax(genericName.Identifier),
                    genericName.TypeArgumentList.Arguments.Select(_a => ComplexType.FromSyntax(_a, _ignoreUnboundGenerics))),
            _ => throw new ArgumentException("Not a simple type", nameof(_syntax))
        };

        public static SimpleType Parse(string _type, bool _ignoreUnboundGenerics = false)
            => FromSyntax(SyntaxFactory.ParseTypeName(_type), _ignoreUnboundGenerics);

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
                return new SimpleType(s_typenames[_type]);
            }
            else
            {
                string? name = _type.Name;
                int length = _type.Name.IndexOf('`');
                if (length > 0)
                {
                    name = _type.Name.Substring(0, length);
                }
                return new SimpleType(name, _type.GenericTypeArguments.Select(_a => ComplexType.FromType(_a, _ignoreUnboundGenerics)));
            }
        }

        public static implicit operator SimpleType(string _type) => Parse(_type);
        public static implicit operator SimpleType(Type _type) => FromType(_type);

        public SimpleType(Identifier _name, IEnumerable<ComplexType>? _typeArguments = null)
        {
            Name = _name;
            TypeArguments = _typeArguments.EmptyIfNull().ToImmutableArray();
            if (TypeArguments.AnyNull())
            {
                throw new ArgumentException("Null type argument", nameof(_name));
            }
        }

        public Identifier Name { get; }
        public ImmutableArray<ComplexType> TypeArguments { get; }

        public override bool Equals(object? _obj) => Equals(_obj as SimpleType);
        public bool Equals(SimpleType? _other) => _other is not null && Name == _other.Name && TypeArguments.SequenceEqual(_other.TypeArguments);
        public override int GetHashCode() => Identity.CombineHash(Name, Identity.CombineHashes(TypeArguments));
        public static bool operator ==(SimpleType? _left, SimpleType? _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(SimpleType? _left, SimpleType? _right) => !(_left == _right);
        public override string ToString() => TypeArguments.Any() ? $"{Name}<{string.Join(',', TypeArguments)}>" : $"{Name}";

        public SimpleType WithName(Identifier _name)
            => _name == Name ? this : new SimpleType(_name, TypeArguments);

        public SimpleType WithTypeArguments(params ComplexType[] _typeArguments)
            => WithTypeArguments((IEnumerable<ComplexType>) _typeArguments);

        public SimpleType WithTypeArguments(IEnumerable<ComplexType>? _typeArguments = null)
            => TypeArguments.Equals(_typeArguments) ? this : new SimpleType(Name, _typeArguments);

    }

}



