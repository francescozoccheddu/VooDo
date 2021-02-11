using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Names
{

    public sealed record SimpleType(Identifier Name, ImmutableArray<ComplexType> TypeArguments = default) : Node
    {

        #region Creation

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
                    genericName.TypeArgumentList.Arguments.Select(_a => ComplexType.FromSyntax(_a, _ignoreUnboundGenerics)).ToImmutableArray()),
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

        #endregion

        #region Conversion

        public static implicit operator SimpleType(string _type) => Parse(_type);
        public static implicit operator SimpleType(Identifier _name) => new SimpleType(_name);
        public static implicit operator SimpleType(Type _type) => FromType(_type);
        public static implicit operator string(SimpleType _simpleType) => _simpleType.ToString();

        #endregion

        #region Delegating constructors

        public SimpleType(Identifier _name, params ComplexType[] _typeArguments) : this(_name, _typeArguments.ToImmutableArray()) { }
        public SimpleType(Identifier _name, IEnumerable<ComplexType>? _typeArguments) : this(_name, _typeArguments.EmptyIfNull().ToImmutableArray()) { }

        #endregion

        #region Members

        private ImmutableArray<ComplexType> m_typeArguments = TypeArguments.EmptyIfDefault();
        public ImmutableArray<ComplexType> TypeArguments
        {
            get => m_typeArguments;
            init => m_typeArguments = value.EmptyIfDefault();
        }
        public bool IsGeneric => !TypeArguments.IsEmpty;

        #endregion

        #region Overrides

        internal override SimpleNameSyntax EmitNode(Scope _scope, Marker _marker)
            => (IsGeneric
            ? (SimpleNameSyntax) SyntaxFactory.GenericName(
                Name.EmitToken(_marker),
                SyntaxFactoryHelper.TypeArguments(
                    TypeArguments.Select(_a => _a.EmitNode(_scope, _marker))))
            : SyntaxFactory.IdentifierName(Name.EmitToken(_marker)))
            .Own(_marker, this);
        public override IEnumerable<NodeOrIdentifier> Children => new NodeOrIdentifier[] { Name }.Concat(TypeArguments);
        public override string ToString() => IsGeneric ? $"{Name}<{string.Join(", ", TypeArguments)}>" : $"{Name}";

        #endregion

    }

}



