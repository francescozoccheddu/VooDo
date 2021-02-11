using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Runtime;

namespace VooDo.Utils
{

    internal static class SyntaxFactoryHelper
    {

        internal static TypeSyntax VariableType(TypeSyntax _type)
        {
            QualifiedType variableType = QualifiedType.FromType<Variable<object>>() with
            {
                Alias = Linker.runtimeReferenceExternAlias
            };
            QualifiedNameSyntax syntax = (QualifiedNameSyntax) variableType.EmitNode(new Scope(), new Marker());
            GenericNameSyntax right = (GenericNameSyntax) syntax.Right;
            right = right.WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(_type)));
            return syntax.WithRight(right);
        }

        internal static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax _source, SimpleNameSyntax _member)
            => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, _source, _member);

        internal static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax _source, string _member)
            => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, _source, SyntaxFactory.IdentifierName(_member));

        internal static MemberAccessExpressionSyntax ThisMemberAccess(SimpleNameSyntax _member)
            => MemberAccess(SyntaxFactory.ThisExpression(), _member);

        internal static MemberAccessExpressionSyntax ThisMemberAccess(string _name)
            => ThisMemberAccess(SyntaxFactory.IdentifierName(_name));

        internal static ArgumentListSyntax Arguments(IEnumerable<ArgumentSyntax> _arguments)
            => SyntaxFactory.ArgumentList(_arguments.ToSeparatedList());

        internal static BracketedArgumentListSyntax BracketedArguments(IEnumerable<ArgumentSyntax> _arguments)
            => SyntaxFactory.BracketedArgumentList(_arguments.ToSeparatedList());

        internal static TypeArgumentListSyntax TypeArguments(IEnumerable<TypeSyntax> _typeArguments)
            => SyntaxFactory.TypeArgumentList(_typeArguments.ToSeparatedList());

        internal static SeparatedSyntaxList<TNode> ToSeparatedList<TNode>(this IEnumerable<TNode> _nodes) where TNode : SyntaxNode
            => SyntaxFactory.SeparatedList(_nodes);

        internal static SyntaxList<TNode> ToSyntaxList<TNode>(this IEnumerable<TNode> _nodes) where TNode : SyntaxNode
            => SyntaxFactory.List(_nodes);

        internal static TypeSyntax ProgramType(TypeSyntax? _returnType)
        {
            if (_returnType is null)
            {
                return (QualifiedType.FromType<Program>() with
                {
                    Alias = Linker.runtimeReferenceExternAlias
                }).EmitNode(new Scope(), new Marker());
            }
            else
            {
                QualifiedType programType = QualifiedType.FromType<Variable<Program>>() with
                {
                    Alias = Linker.runtimeReferenceExternAlias
                };
                QualifiedNameSyntax syntax = (QualifiedNameSyntax) programType.EmitNode(new Scope(), new Marker());
                GenericNameSyntax right = (GenericNameSyntax) syntax.Right;
                right = right.WithTypeArgumentList(
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(_returnType)));
                return syntax.WithRight(right);
            }
        }

    }

}
