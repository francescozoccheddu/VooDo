using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VooDo.Language.AST.Names
{

    public static class SyntaxHelper
    {

        internal static SyntaxToken ToSyntaxToken(this Identifier _identifier)
            => SyntaxFactory.Identifier(_identifier);

        internal static IdentifierNameSyntax ToNameSyntax(this Identifier _identifier)
            => SyntaxFactory.IdentifierName(_identifier);

        internal static SimpleNameSyntax ToNameSyntax(this SimpleType _simpleType)
            => (SimpleNameSyntax) SyntaxFactory.ParseName(_simpleType.ToString());

        public static QualifiedType Specialize(this QualifiedType _qualifiedType, params ComplexType[] _typeArguments)
            => _qualifiedType.Specialize((IEnumerable<QualifiedType>) _typeArguments);

        public static QualifiedType Specialize(this QualifiedType _qualifiedType, IEnumerable<ComplexType> _typeArguments)
            => _qualifiedType with
            {
                Path = _qualifiedType.Path.SkipLast(1).Append(
                    _qualifiedType.Path.Last() with
                    {
                        TypeArguments = _typeArguments.ToImmutableArray()
                    }).ToImmutableArray()
            };

        public static SimpleType ToSimpleType(this QualifiedType _type)
            => _type.Path[0];

        internal static TypeSyntax ToTypeSyntax(this ComplexType _qualifiedType)
            => SyntaxFactory.ParseTypeName(_qualifiedType.ToString());

        internal static TypeSyntax ToTypeSyntax(this ComplexTypeOrVar _qualifiedTypeOrVar)
            => _qualifiedTypeOrVar.IsVar
            ? SyntaxFactory.IdentifierName("var")
            : SyntaxFactory.ParseTypeName(_qualifiedTypeOrVar.ToString());

        internal static NameSyntax ToNameSyntax(this Namespace _namespace)
            => SyntaxFactory.ParseName(_namespace.ToString());

        internal static NameSyntax Specialize(NameSyntax _type, params TypeSyntax[] _typeArguments)
            => Specialize(_type, _typeArguments);

        internal static NameSyntax Specialize(NameSyntax _type, IEnumerable<TypeSyntax> _typeArguments)
        {
            SimpleNameSyntax node = _type switch
            {
                QualifiedNameSyntax qualifiedName => qualifiedName.Right,
                AliasQualifiedNameSyntax aliasQualifiedName => aliasQualifiedName.Name,
                SimpleNameSyntax simpleNameSyntax => simpleNameSyntax,
                _ => throw new ArgumentException("Unexpected NameSyntax type", nameof(_type)),
            };
            return _type.ReplaceNode(node, SyntaxFactory.GenericName(node.Identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(_typeArguments))));
        }

    }

}
