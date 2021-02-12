using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Linq;

using VooDo.Compilation;
using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Runtime;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace VooDo.Utils
{

    internal static class SyntaxFactoryHelper
    {

        private static readonly QualifiedNameSyntax s_programType = (QualifiedNameSyntax) (QualifiedType.FromType<Program>() with
        {
            Alias = Compiler.runtimeReferenceAlias
        }).ToTypeSyntax();

        private static readonly QualifiedNameSyntax s_genericProgramType = (QualifiedNameSyntax) (QualifiedType.FromType<Program<object>>() with
        {
            Alias = Compiler.runtimeReferenceAlias
        }).ToTypeSyntax();

        private static readonly QualifiedNameSyntax s_variableType = (QualifiedNameSyntax) (QualifiedType.FromType<Variable>() with
        {
            Alias = Compiler.runtimeReferenceAlias
        }).ToTypeSyntax();

        private static readonly QualifiedNameSyntax s_genericVariableType = (QualifiedNameSyntax) (QualifiedType.FromType<Variable<object>>() with
        {
            Alias = Compiler.runtimeReferenceAlias
        }).ToTypeSyntax();

        private static readonly QualifiedNameSyntax s_runtimeHelpersType = (QualifiedNameSyntax) (QualifiedType.FromType(typeof(RuntimeHelpers)) with
        {
            Alias = Compiler.runtimeReferenceAlias
        }).ToTypeSyntax();

        internal static TypeSyntax ToTypeSyntax(this ComplexType _type)
            => _type.EmitNode(new Scope(), new Marker());

        internal static InvocationExpressionSyntax CreateVariableInvocation(TypeSyntax? _type, LiteralExpressionSyntax _name, ExpressionSyntax _initialValue)
            => Invocation(
                MemberAccess(
                    s_runtimeHelpersType,
                    GenericName(
                        nameof(RuntimeHelpers.CreateVariable),
                        _type!)),
                    _name,
                    _initialValue);

        internal static InvocationExpressionSyntax SetControllerAndGetValueInvocation(ExpressionSyntax _variable, ExpressionSyntax _controller)
            => Invocation(
                MemberAccess(
                    s_runtimeHelpersType,
                    nameof(RuntimeHelpers.SetControllerAndGetValue)),
                _variable,
                _controller);

        internal static TypeSyntax VariableType(TypeSyntax? _type = null)
        {
            if (_type is null)
            {
                return s_variableType;
            }
            else
            {
                QualifiedNameSyntax syntax = s_genericVariableType;
                GenericNameSyntax right = (GenericNameSyntax) syntax.Right;
                right = right.WithTypeArgumentList(
                    SF.TypeArgumentList(
                        SF.SingletonSeparatedList(_type)));
                return syntax.WithRight(right);
            }
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

        internal static TypeArgumentListSyntax TypeArguments(params TypeSyntax[] _typeArguments)
            => TypeArguments((IEnumerable<TypeSyntax>) _typeArguments);

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

        internal static TypeSyntax ProgramType(TypeSyntax? _returnType = null)
        {
            if (_returnType is null)
            {
                return s_programType;
            }
            else
            {
                QualifiedNameSyntax syntax = s_genericProgramType;
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

        internal static InvocationExpressionSyntax Invocation(ExpressionSyntax _source, params ExpressionSyntax[] _arguments)
            => Invocation(_source, _arguments.Select(_a => SF.Argument(_a)));

        internal static InvocationExpressionSyntax Invocation(ExpressionSyntax _source, IEnumerable<ArgumentSyntax> _arguments)
            => SF.InvocationExpression(
                _source,
                Arguments(_arguments));

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

        internal static PredefinedTypeSyntax Void()
            => SF.PredefinedType(SF.Token(SyntaxKind.VoidKeyword));

        internal static PredefinedTypeSyntax Object()
            => SF.PredefinedType(SF.Token(SyntaxKind.ObjectKeyword));

        internal static NameSyntax? QualifiedName(IEnumerable<NameSyntax> _names)
        {
            NameSyntax? type = null;
            foreach (NameSyntax name in _names)
            {
                type = type is null
                    ? name
                    : SF.QualifiedName(type, (SimpleNameSyntax) name);
            }
            return type;
        }

        internal static PropertyDeclarationSyntax ArrowProperty(TypeSyntax _type, string _name, ExpressionSyntax _expression)
            => SF.PropertyDeclaration(_type, _name)
                .WithExpressionBody(SF.ArrowExpressionClause(_expression))
                .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken));

    }

}
