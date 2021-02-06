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

        public static SimpleType FromSyntax(TypeSyntax _syntax)
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            if (_syntax is SimpleNameSyntax simple)
            {
                return FromSyntax(simple);
            }
            else if (_syntax is PredefinedTypeSyntax predefined)
            {
                return FromSyntax(predefined);
            }
            else
            {
                throw new ArgumentException("Not a simple type", nameof(_syntax));
            }
        }

        public static SimpleType FromSyntax(PredefinedTypeSyntax _syntax)
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            return new SimpleType(Identifier.FromSyntax(_syntax.Keyword));
        }

        public static SimpleType FromSyntax(SimpleNameSyntax _syntax)
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            if (_syntax is IdentifierNameSyntax name)
            {
                return new SimpleType(Identifier.FromSyntax(name.Identifier));
            }
            else if (_syntax is GenericNameSyntax genericName)
            {
                return new SimpleType(
                    Identifier.FromSyntax(genericName.Identifier),
                    genericName.TypeArgumentList.Arguments.Select(QualifiedType.FromSyntax));
            }
            else
            {
                throw new ArgumentException("Not a simple type", nameof(_syntax));
            }
        }

        public static SimpleType Parse(string _type)
        {
            if (_type == null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            TypeSyntax syntax = SyntaxFactory.ParseTypeName(_type);
            if (syntax is SimpleNameSyntax simpleSyntax)
            {
                return FromSyntax(simpleSyntax);
            }
            else if (syntax is PredefinedTypeSyntax predefinedSyntax)
            {
                return FromSyntax(predefinedSyntax);
            }
            else
            {
                throw new ArgumentException("Not a simple type", nameof(_type));
            }
        }

        public static SimpleType FromType<TType>()
            => FromType(typeof(TType));

        public static SimpleType FromType(Type _type)
        {
            if (_type == null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            if (_type.IsGenericTypeDefinition)
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
                string name = _type.Name.Substring(0, _type.Name.IndexOf('`'));
                return new SimpleType(name, _type.GenericTypeArguments.Select(QualifiedType.FromType));
            }
        }

        public static implicit operator SimpleType(string _type) => Parse(_type);
        public static implicit operator SimpleType(Type _type) => FromType(_type);

        public SimpleType(Identifier _name, IEnumerable<QualifiedType> _typeArguments = null)
        {
            if (_name == null)
            {
                throw new ArgumentNullException(nameof(_name));
            }
            Name = _name;
            TypeArguments = _typeArguments.EmptyIfNull().ToImmutableArray();
            if (TypeArguments.AnyNull())
            {
                throw new ArgumentException("Null type argument", nameof(_name));
            }
        }

        public Identifier Name { get; }
        public ImmutableArray<QualifiedType> TypeArguments { get; }

        public override bool Equals(object _obj) => Equals(_obj as SimpleType);
        public bool Equals(SimpleType _other) => _other != null && Name == _other.Name && TypeArguments.SequenceEqual(_other.TypeArguments);
        public override int GetHashCode() => Identity.CombineHash(Name, Identity.CombineHashes(TypeArguments));
        public static bool operator ==(SimpleType _left, SimpleType _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(SimpleType _left, SimpleType _right) => !(_left == _right);
        public override string ToString() => TypeArguments.Any() ? $"{Name}<{string.Join(',', TypeArguments)}>" : $"{Name}";

        public SimpleType WithName(Identifier _name)
            => _name == Name ? this : new SimpleType(_name, TypeArguments);

        public SimpleType WithTypeArguments(params QualifiedType[] _typeArguments)
            => WithTypeArguments((IEnumerable<QualifiedType>) _typeArguments);

        public SimpleType WithTypeArguments(IEnumerable<QualifiedType> _typeArguments = null)
            => TypeArguments.SequenceEqual(_typeArguments) ? this : new SimpleType(Name, _typeArguments);

    }

}



