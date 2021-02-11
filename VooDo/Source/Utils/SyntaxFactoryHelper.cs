using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Runtime;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace VooDo.Utils
{

    internal static class SyntaxFactoryHelper
    {

        internal static TypeSyntax ToTypeSyntax(this ComplexType _type)
            => _type.EmitNode(new Scope(), new Marker());

        internal static TypeSyntax VariableType()
        {
            QualifiedType variableType = QualifiedType.FromType<Variable>() with
            {
                Alias = Linker.runtimeReferenceExternAlias
            };
            return variableType.ToTypeSyntax();
        }

        internal static TypeSyntax VariableType(TypeSyntax _type)
        {
            QualifiedType variableType = QualifiedType.FromType<Variable<object>>() with
            {
                Alias = Linker.runtimeReferenceExternAlias
            };
            QualifiedNameSyntax syntax = (QualifiedNameSyntax) variableType.ToTypeSyntax();
            GenericNameSyntax right = (GenericNameSyntax) syntax.Right;
            right = right.WithTypeArgumentList(
                SF.TypeArgumentList(
                    SF.SingletonSeparatedList(_type)));
            return syntax.WithRight(right);
        }

        internal static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax _source, SyntaxToken _member)
            => MemberAccess(_source, SF.IdentifierName(_member));

        internal static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax _source, SimpleNameSyntax _member)
            => SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, _source, _member);

        internal static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax _source, string _member)
            => MemberAccess(_source, SF.Identifier(_member));

        internal static MemberAccessExpressionSyntax ThisMemberAccess(SyntaxToken _member)
            => ThisMemberAccess(SF.IdentifierName(_member));

        internal static MemberAccessExpressionSyntax ThisMemberAccess(SimpleNameSyntax _member)
            => MemberAccess(SF.ThisExpression(), _member);

        internal static MemberAccessExpressionSyntax ThisMemberAccess(string _name)
            => ThisMemberAccess(SF.IdentifierName(_name));

        internal static ArgumentListSyntax Arguments(IEnumerable<ArgumentSyntax> _arguments)
            => SF.ArgumentList(_arguments.ToSeparatedList());

        internal static BracketedArgumentListSyntax BracketedArguments(IEnumerable<ArgumentSyntax> _arguments)
            => SF.BracketedArgumentList(_arguments.ToSeparatedList());

        internal static TypeArgumentListSyntax TypeArguments(IEnumerable<TypeSyntax> _typeArguments)
            => SF.TypeArgumentList(_typeArguments.ToSeparatedList());

        internal static SeparatedSyntaxList<TNode> ToSeparatedList<TNode>(this TNode _node) where TNode : SyntaxNode
            => SF.SingletonSeparatedList(_node);

        internal static SyntaxList<TNode> ToSyntaxList<TNode>(this TNode _node) where TNode : SyntaxNode
            => SF.SingletonList(_node);

        internal static SeparatedSyntaxList<TNode> ToSeparatedList<TNode>(this IEnumerable<TNode> _nodes) where TNode : SyntaxNode
        => SF.SeparatedList(_nodes);

        internal static SyntaxList<TNode> ToSyntaxList<TNode>(this IEnumerable<TNode> _nodes) where TNode : SyntaxNode
            => SF.List(_nodes);

        internal static TypeSyntax ProgramType(TypeSyntax? _returnType)
        {
            if (_returnType is null)
            {
                return (QualifiedType.FromType<Program>() with
                {
                    Alias = Linker.runtimeReferenceExternAlias
                }).ToTypeSyntax();
            }
            else
            {
                QualifiedType programType = QualifiedType.FromType<Variable<Program>>() with
                {
                    Alias = Linker.runtimeReferenceExternAlias
                };
                QualifiedNameSyntax syntax = (QualifiedNameSyntax) programType.ToTypeSyntax();
                GenericNameSyntax right = (GenericNameSyntax) syntax.Right;
                right = right.WithTypeArgumentList(
                    SF.TypeArgumentList(
                        SF.SingletonSeparatedList(_returnType)));
                return syntax.WithRight(right);
            }
        }

        internal static SyntaxTokenList Tokens(params SyntaxKind[] _kinds)
            => SF.TokenList(_kinds.Select(_k => SF.Token(_k)));



        internal static ArrayRankSpecifierSyntax ArrayRank(int _rank)
            => SF.ArrayRankSpecifier(
                Enumerable.Repeat<ExpressionSyntax>(
                    SF.OmittedArraySizeExpression(), _rank)
                .ToSeparatedList());

        internal static SyntaxList<ArrayRankSpecifierSyntax> ArrayRanks(IEnumerable<int> _ranks)
            => _ranks.Select(ArrayRank).ToSyntaxList();

        internal static SyntaxList<ArrayRankSpecifierSyntax> ArrayRanks(IEnumerable<ExpressionSyntax> _sizes, IEnumerable<int> _additionalRanks)
            => new[] { SF.ArrayRankSpecifier(_sizes.ToSeparatedList()) }
            .Concat(ArrayRanks(_additionalRanks))
            .ToSyntaxList();

        internal static SyntaxList<ArrayRankSpecifierSyntax> SingleArrayRank()
            => ArrayRanks(new[] { 1 });

        internal static GenericNameSyntax GenericName(string _name, params TypeSyntax[] _typeArguments)
            => GenericName(SF.Identifier(_name), _typeArguments);

        internal static GenericNameSyntax GenericName(SyntaxToken _name, params TypeSyntax[] _typeArguments)
            => GenericName(_name, (IEnumerable<TypeSyntax>) _typeArguments);

        internal static GenericNameSyntax GenericName(SyntaxToken _name, IEnumerable<TypeSyntax> _typeArguments)
            => SF.GenericName(_name, TypeArguments(_typeArguments));

        internal static EqualsValueClauseSyntax ToEqualsValueClause(this ExpressionSyntax _expression)
            => SF.EqualsValueClause(_expression);

    }

}
