﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Runtime;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SK = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace VooDo.Utils
{

    internal static class SyntaxFactoryUtils
    {

        private static readonly ImmutableHashSet<string> s_keywords = new[] {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
            "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
            "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
            "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
            "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
            "unsafe", "ushort", "using", "virtual", "void", "volatile", "while" }
        .ToImmutableHashSet();

        private static readonly ImmutableDictionary<string, SyntaxToken> s_predefinedTypesTokens =
            new SK[] {
                SK.BoolKeyword,
                SK.CharKeyword,
                SK.StringKeyword,
                SK.ByteKeyword,
                SK.SByteKeyword,
                SK.ShortKeyword,
                SK.UShortKeyword,
                SK.IntKeyword,
                SK.UIntKeyword,
                SK.LongKeyword,
                SK.ULongKeyword,
                SK.DecimalKeyword,
                SK.FloatKeyword,
                SK.DoubleKeyword,
                SK.ObjectKeyword
            }.Select(_k => SF.Token(_k))
            .ToImmutableDictionary(_t => _t.ValueText);

        private static QualifiedNameSyntax ProgramType(Identifier _runtimeAlias) => (QualifiedNameSyntax)(QualifiedType.FromType<Program>() with
        {
            Alias = _runtimeAlias
        }).ToTypeSyntax();

        private static QualifiedNameSyntax GenericProgramType(Identifier _runtimeAlias) => (QualifiedNameSyntax)(QualifiedType.FromType<TypedProgram<object>>() with
        {
            Alias = _runtimeAlias
        }).ToTypeSyntax();

        private static QualifiedNameSyntax VariableType(Identifier _runtimeAlias) => (QualifiedNameSyntax)(QualifiedType.FromType<Variable>() with
        {
            Alias = _runtimeAlias
        }).ToTypeSyntax();

        private static QualifiedNameSyntax GenericVariableType(Identifier _runtimeAlias) => (QualifiedNameSyntax)(QualifiedType.FromType<Variable<object>>() with
        {
            Alias = _runtimeAlias
        }).ToTypeSyntax();

        private static QualifiedNameSyntax RuntimeHelpersType(Identifier _runtimeAlias) => (QualifiedNameSyntax)(QualifiedType.FromType(typeof(RuntimeHelpers)) with
        {
            Alias = _runtimeAlias
        }).ToTypeSyntax();

        internal static PredefinedTypeSyntax PredefinedType(SyntaxKind _kind)
            => SF.PredefinedType(SF.Token(_kind));

        internal static TypeSyntax ToTypeSyntax(this ComplexType _type)
            => Emitter.Emit(_type, new Tagger());

        internal static NameSyntax ToNameSyntax(this Namespace _namespace)
            => Emitter.Emit(_namespace, new Tagger());

        internal static InvocationExpressionSyntax CreateVariableInvocation(Identifier _runtimeAlias, TypeSyntax? _type, bool _isConstant, string? _name, ExpressionSyntax _initialValue)
            => Invocation(
                MemberAccess(
                    RuntimeHelpersType(_runtimeAlias),
                    GenericName(
                        nameof(RuntimeHelpers.CreateVariable),
                        _type!)),
                    Literal(_isConstant),
                    Literal(_name),
                    _initialValue);

        internal static InvocationExpressionSyntax SetControllerAndGetValueInvocation(Identifier _runtimeAlias, ExpressionSyntax _variable, ExpressionSyntax _controller)
            => Invocation(
                MemberAccess(
                    RuntimeHelpersType(_runtimeAlias),
                    nameof(RuntimeHelpers.SetControllerAndGetValue)),
                _variable,
                _controller);

        internal static InvocationExpressionSyntax SubscribeHookInvocation(ExpressionSyntax _source, int _setIndex, int _hookIndex)
            => Invocation(
                ThisMemberAccess(RuntimeHelpers.subscribeHookMethodName),
                _source,
                Literal(_setIndex),
                Literal(_hookIndex));

        internal static TypeSyntax VariableType(Identifier _runtimeAlias, TypeSyntax? _type = null)
        {
            if (_type is null)
            {
                return VariableType(_runtimeAlias);
            }
            else
            {
                QualifiedNameSyntax syntax = GenericVariableType(_runtimeAlias);
                GenericNameSyntax right = (GenericNameSyntax)syntax.Right;
                right = right.WithTypeArgumentList(
                    SF.TypeArgumentList(
                        SF.SingletonSeparatedList(_type)));
                return syntax.WithRight(right);
            }
        }

        internal static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax _source, SyntaxToken _member)
            => MemberAccess(_source, SF.IdentifierName(_member));

        internal static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax _source, SimpleNameSyntax _member)
            => SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, _source, _member);

        internal static MemberAccessExpressionSyntax MemberAccess(ExpressionSyntax _source, string _member)
            => MemberAccess(_source, Identifier(_member));

        internal static MemberAccessExpressionSyntax ThisMemberAccess(SyntaxToken _member)
            => ThisMemberAccess(SF.IdentifierName(_member));

        internal static MemberAccessExpressionSyntax ThisMemberAccess(SimpleNameSyntax _member)
            => MemberAccess(SF.ThisExpression(), _member);

        internal static MemberAccessExpressionSyntax ThisMemberAccess(string _name)
            => ThisMemberAccess(SF.IdentifierName(_name));

        internal static ArgumentListSyntax Arguments(params ExpressionSyntax[] _arguments)
            => Arguments(_arguments.Select(_a => SF.Argument(_a)));

        internal static ArgumentListSyntax Arguments(IEnumerable<ArgumentSyntax> _arguments)
            => SF.ArgumentList(_arguments.ToSeparatedList());

        internal static BracketedArgumentListSyntax BracketedArguments(IEnumerable<ArgumentSyntax> _arguments)
            => SF.BracketedArgumentList(_arguments.ToSeparatedList());

        internal static TypeArgumentListSyntax TypeArguments(params TypeSyntax[] _typeArguments)
            => TypeArguments((IEnumerable<TypeSyntax>)_typeArguments);

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

        internal static TypeSyntax ProgramType(Identifier _runtimeAlias, TypeSyntax? _returnType = null)
        {
            if (_returnType is null)
            {
                return ProgramType(_runtimeAlias);
            }
            else
            {
                QualifiedNameSyntax syntax = GenericProgramType(_runtimeAlias);
                GenericNameSyntax right = (GenericNameSyntax)syntax.Right;
                right = right.WithTypeArgumentList(
                    SF.TypeArgumentList(
                        SF.SingletonSeparatedList(_returnType)));
                return syntax.WithRight(right);
            }
        }

        internal static SyntaxTokenList Tokens(params SK[] _kinds)
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

        internal static LiteralExpressionSyntax NullLiteral { get; } = SF.LiteralExpression(SK.NullLiteralExpression);

        internal static LiteralExpressionSyntax Literal(string? _literal)
            => _literal is null
                ? NullLiteral
                : SF.LiteralExpression(SK.StringLiteralExpression, SF.Literal(_literal));

        internal static LiteralExpressionSyntax Literal(int _literal)
            => SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(_literal));

        internal static LiteralExpressionSyntax Literal(bool _literal)
            => SF.LiteralExpression(_literal ? SK.TrueLiteralExpression : SK.FalseLiteralExpression);

        internal static InvocationExpressionSyntax Invocation(ExpressionSyntax _source, params ExpressionSyntax[] _arguments)
            => SF.InvocationExpression(
                _source,
                Arguments(_arguments));

        internal static InvocationExpressionSyntax Invocation(ExpressionSyntax _source, IEnumerable<ArgumentSyntax> _arguments)
            => SF.InvocationExpression(
                _source,
                Arguments(_arguments));

        internal static TupleTypeSyntax TupleType(params TypeSyntax[] _types)
            => SF.TupleType(_types.Select(_t => SF.TupleElement(_t)).ToSeparatedList());

        internal static ArrayTypeSyntax SingleArray(TypeSyntax _syntax)
            => SF.ArrayType(_syntax, SingleArrayRank());

        internal static SyntaxList<ArrayRankSpecifierSyntax> SingleArrayRank()
            => ArrayRanks(new[] { 1 });

        internal static GenericNameSyntax GenericName(string _name, params TypeSyntax[] _typeArguments)
            => GenericName(Identifier(_name), _typeArguments);

        internal static GenericNameSyntax GenericName(SyntaxToken _name, params TypeSyntax[] _typeArguments)
            => GenericName(_name, (IEnumerable<TypeSyntax>)_typeArguments);

        internal static GenericNameSyntax GenericName(SyntaxToken _name, IEnumerable<TypeSyntax> _typeArguments)
            => SF.GenericName(_name, TypeArguments(_typeArguments));

        internal static EqualsValueClauseSyntax ToEqualsValueClause(this ExpressionSyntax _expression)
            => SF.EqualsValueClause(_expression);

        internal static PredefinedTypeSyntax Void()
            => SF.PredefinedType(SF.Token(SK.VoidKeyword));

        internal static PredefinedTypeSyntax Object()
            => SF.PredefinedType(SF.Token(SK.ObjectKeyword));

        internal static NameSyntax? QualifiedName(IEnumerable<NameSyntax> _names)
        {
            NameSyntax? type = null;
            foreach (NameSyntax name in _names)
            {
                type = type is null
                    ? name
                    : SF.QualifiedName(type, (SimpleNameSyntax)name);
            }
            return type;
        }

        internal static PropertyDeclarationSyntax ArrowProperty(TypeSyntax _type, string _name, ExpressionSyntax _expression)
            => SF.PropertyDeclaration(_type, _name)
                .WithExpressionBody(SF.ArrowExpressionClause(_expression))
                .WithSemicolonToken(SF.Token(SK.SemicolonToken));

        internal static SyntaxToken Identifier(string _name)
            => SF.Identifier(s_keywords.Contains(_name) ? '@' + _name.TrimStart('@') : _name);

        internal static SyntaxToken AliasIdentifier(string _name)
            => _name == "global"
            ? SF.Token(SK.GlobalKeyword)
            : Identifier(_name);

        internal static TypeSyntax TypeIdentifier(string _name)
            => s_predefinedTypesTokens.TryGetValue(_name, out SyntaxToken token)
            ? SF.PredefinedType(token)
            : SF.IdentifierName(Identifier(_name));


    }

}