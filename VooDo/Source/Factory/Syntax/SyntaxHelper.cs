using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

namespace VooDo.Factory.Syntax
{

    internal static class SyntaxHelper
    {

        internal static SyntaxToken ToSyntaxToken(this Identifier _identifier)
            => SyntaxFactory.Identifier(_identifier);

        internal static IdentifierNameSyntax ToNameSyntax(this Identifier _identifier)
            => SyntaxFactory.IdentifierName(_identifier);

        internal static SimpleNameSyntax ToNameSyntax(this SimpleType _simpleType)
            => (SimpleNameSyntax) SyntaxFactory.ParseName(_simpleType.ToString());

        internal static QualifiedType Specialize(this QualifiedType _qualifiedType, params ComplexType[] _typeArguments)
            => _qualifiedType.Specialize((IEnumerable<QualifiedType>) _typeArguments);

        internal static QualifiedType Specialize(this QualifiedType _qualifiedType, IEnumerable<ComplexType> _typeArguments)
            => _qualifiedType.WithPath(_qualifiedType.Path.SkipLast(1).Append(_qualifiedType.Path.Last().WithTypeArguments(_typeArguments)));

        internal static TypeSyntax ToTypeSyntax(this ComplexType _qualifiedType)
            => SyntaxFactory.ParseTypeName(_qualifiedType.ToString());

        internal static TypeSyntax ToTypeSyntax(this ComplexTypeOrVar _qualifiedTypeOrVar)
            => _qualifiedTypeOrVar.IsVar
            ? SyntaxFactory.IdentifierName("var")
            : SyntaxFactory.ParseTypeName(_qualifiedTypeOrVar.ToString());

        internal static NameSyntax ToNameSyntax(this Namespace _namespace)
            => SyntaxFactory.ParseName(_namespace.ToString());

        internal static NameSyntax Specialize(NameSyntax _type, params TypeSyntax[] _typeArguments)
            => Specialize(_type, (IEnumerable<TypeSyntax>) _typeArguments);

        internal static NameSyntax Specialize(NameSyntax _type, IEnumerable<TypeSyntax> _typeArguments)
        {
            SimpleNameSyntax node;
            if (_type is QualifiedNameSyntax qualifiedName)
            {
                node = qualifiedName.Right;
            }
            else if (_type is AliasQualifiedNameSyntax aliasQualifiedName)
            {
                node = aliasQualifiedName.Name;
            }
            else if (_type is SimpleNameSyntax simpleNameSyntax)
            {
                node = simpleNameSyntax;
            }
            else
            {
                throw new ArgumentException("Unexpected NameSyntax type", nameof(_type));
            }
            return _type.ReplaceNode(node, SyntaxFactory.GenericName(node.Identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(_typeArguments))));
        }

    }

}
