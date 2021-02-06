using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

using VooDo.Utils;

namespace VooDo.Factory.Syntax
{

    public sealed class QualifiedTypeOrVar : IEquatable<QualifiedTypeOrVar>
    {

        public static QualifiedTypeOrVar Var { get; } = new QualifiedTypeOrVar(null);

        public static QualifiedTypeOrVar Parse(string _type)
        {
            if (_type == null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            return FromSyntax(SyntaxFactory.ParseTypeName(_type));
        }

        public static QualifiedTypeOrVar FromType(Type _type)
            => QualifiedType.FromType(_type);

        public static QualifiedTypeOrVar FromType<TType>()
            => FromType(typeof(TType));

        public static QualifiedTypeOrVar FromSyntax(TypeSyntax _type)
        {
            if (_type is IdentifierNameSyntax name && name.IsVar)
            {
                return Var;
            }
            else
            {
                return QualifiedType.FromSyntax(_type);
            }
        }

        public static QualifiedTypeOrVar FromQualifiedType(QualifiedType _type)
            => new QualifiedTypeOrVar(_type);

        public static implicit operator QualifiedTypeOrVar(QualifiedType _qualifiedType) => FromQualifiedType(_qualifiedType);
        public static implicit operator QualifiedTypeOrVar(Identifier _identifier) => new QualifiedType(_identifier);
        public static implicit operator QualifiedTypeOrVar(SimpleType _simpleType) => new QualifiedType(_simpleType);
        public static implicit operator QualifiedTypeOrVar(string _type) => Parse(_type);
        public static implicit operator QualifiedTypeOrVar(Type _type) => FromType(_type);

        public override bool Equals(object _obj) => Equals(_obj as QualifiedTypeOrVar);
        public bool Equals(QualifiedTypeOrVar _other) => _other != null && Type == _other.Type;
        public override int GetHashCode() => Identity.CombineHash(Type);
        public static bool operator ==(QualifiedTypeOrVar _left, QualifiedTypeOrVar _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(QualifiedTypeOrVar _left, QualifiedTypeOrVar _right) => !(_left == _right);
        public override string ToString() => this == Var ? "var" : Type.ToString();

        private QualifiedTypeOrVar(QualifiedType _type)
        {
            Type = _type;
        }

        public QualifiedType Type { get; }

    }

}
