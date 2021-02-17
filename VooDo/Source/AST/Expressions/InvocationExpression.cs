using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public record InvocationExpression(InvocationExpression.Callable Source, ImmutableArray<Argument> Arguments = default) : InvocationOrObjectCreationExpression
    {

        #region Nested types

        public abstract record Callable : BodyNode
        {

#if NET5_0
            public sealed override InvocationExpression? Parent => (InvocationExpression?) base.Parent;
#endif

        }

        public sealed record Method : Callable
        {

            public Method(NameOrMemberAccessExpression _source, ImmutableArray<ComplexType> _typeArguments = default)
            {
                Source = _source;
                TypeArguments = _typeArguments;
            }

            public NameOrMemberAccessExpression Source { get; init; }

            private ImmutableArray<ComplexType> m_typeArguments;
            public ImmutableArray<ComplexType> TypeArguments
            {
                get => m_typeArguments;
                init => m_typeArguments = value.EmptyIfDefault();
            }

            public bool IsGeneric => !TypeArguments.IsEmpty;

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
            {
                NameOrMemberAccessExpression newSource = (NameOrMemberAccessExpression) _map(Source).NonNull();
                ImmutableArray<ComplexType> newArguments = TypeArguments.Map(_map).NonNull();
                if (ReferenceEquals(newSource, Source) && newArguments == TypeArguments)
                {
                    return this;
                }
                else
                {
                    return this with
                    {
                        Source = newSource,
                        TypeArguments = newArguments
                    };
                }
            }

            internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
            {
                ExpressionSyntax source;
                TypeArgumentListSyntax typeArgumentList = SyntaxFactoryUtils.TypeArguments(TypeArguments.Select(_a => (TypeSyntax) _a.EmitNode(_scope, _tagger)));
                if (Source is NameExpression name)
                {
                    SyntaxToken identifier = name.Name.EmitToken(_tagger);
                    source = SyntaxFactory.GenericName(identifier, typeArgumentList);
                }
                else if (Source is MemberAccessExpression member)
                {

                    SyntaxToken identifier = member.Member.EmitToken(_tagger);
                    source = SyntaxFactoryUtils.MemberAccess(
                        (ExpressionSyntax) member.Source.EmitNode(_scope, _tagger),
                        SyntaxFactory.GenericName(identifier, typeArgumentList));
                }
                else
                {
                    throw new InvalidCastException("Unknown Source type");
                }
                return source.Own(_tagger, this);
            }

            public override IEnumerable<Node> Children => new[] { Source };
            public override string ToString() => LeftCode(Source, EPrecedence.Primary) + (IsGeneric ? $"<{string.Join(", ", TypeArguments)}>" : "");

        }

        public sealed record SimpleCallable(Expression Source) : Callable
        {

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
            {
                NameOrMemberAccessExpression newSource = (NameOrMemberAccessExpression) _map(Source).NonNull();
                if (ReferenceEquals(newSource, Source))
                {
                    return this;
                }
                else
                {
                    return this with
                    {
                        Source = newSource
                    };
                }
            }

            internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger) => Source.EmitNode(_scope, _tagger);
            public override IEnumerable<Node> Children => new[] { Source };
            public override string ToString() => LeftCode(Source, EPrecedence.Primary);

        }



        #endregion

        #region Members

        private ImmutableArray<Argument> m_arguments = Arguments.EmptyIfDefault();
        public ImmutableArray<Argument> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.EmptyIfDefault();
        }

        #endregion

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Callable newSource = (Callable) _map(Source).NonNull();
            ImmutableArray<Argument> newArguments = Arguments.Map(_map).NonNull();
            if (ReferenceEquals(newSource, Source) && newArguments == Arguments)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Source = newSource,
                    Arguments = newArguments
                };
            }
        }

        internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactoryUtils.Invocation(
                (ExpressionSyntax) Source.EmitNode(_scope, _tagger),
                Arguments.Select(_a => (ArgumentSyntax) _a.EmitNode(_scope, _tagger)))
            .Own(_tagger, this);
        public override IEnumerable<Node> Children => new BodyNode[] { Source }.Concat(Arguments);
        public override string ToString() => $"{Source}({string.Join(", ", Arguments)})";

        #endregion

    }

}
