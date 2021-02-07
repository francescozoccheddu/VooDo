using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Utils;

namespace VooDo.Factory.Syntax
{

    public sealed class ComplexTypeOrVar : IEquatable<ComplexTypeOrVar>
    {

        public static ComplexTypeOrVar Var { get; } = new ComplexTypeOrVar(null);

        public static ComplexTypeOrVar Parse(string _type, bool _ignoreUnbound = false)
        {
            if (_type is null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            return FromSyntax(SyntaxFactory.ParseTypeName(_type), _ignoreUnbound);
        }

        public static ComplexTypeOrVar FromType(Type _type, bool _ignoreUnbound = false)
            => ComplexType.FromType(_type);

        public static ComplexTypeOrVar FromType<TType>()
            => FromType(typeof(TType));

        public static ComplexTypeOrVar FromSyntax(TypeSyntax _type, bool _ignoreUnbound = false)
        {
            if (_type is IdentifierNameSyntax name && name.IsVar)
            {
                return Var;
            }
            else
            {
                return ComplexType.FromSyntax(_type);
            }
        }

        public static ComplexTypeOrVar FromComplexType(ComplexType _type)
            => new ComplexTypeOrVar(_type);

        public static implicit operator ComplexTypeOrVar(ComplexType _complexType) => FromComplexType(_complexType);
        public static implicit operator ComplexTypeOrVar(Identifier _identifier) => new QualifiedType(_identifier);
        public static implicit operator ComplexTypeOrVar(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator ComplexTypeOrVar(string _type) => Parse(_type);
        public static implicit operator ComplexTypeOrVar(Type _type) => FromType(_type);

        public override bool Equals(object? _obj) => Equals(_obj as ComplexTypeOrVar);
        public bool Equals(ComplexTypeOrVar? _other) => _other is not null && Type == _other.Type;
        public override int GetHashCode() => Identity.CombineHash(Type);
        public static bool operator ==(ComplexTypeOrVar? _left, ComplexTypeOrVar? _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(ComplexTypeOrVar? _left, ComplexTypeOrVar? _right) => !(_left == _right);
        public override string ToString() => this == Var ? "var" : Type.ToString();

        private ComplexTypeOrVar(ComplexType? _type)
        {
            Type = _type;
        }

        public ComplexType? Type { get; }
        public bool IsVar => Type is null;

    }

}
