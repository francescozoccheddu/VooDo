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

            public sealed override InvocationExpression? Parent => (InvocationExpression?) base.Parent;
            public abstract override Callable ReplaceNodes(Func<Node?, Node?> _map);
            internal abstract override ExpressionSyntax EmitNode(Scope _scope, Tagger _tagger);
            public abstract override IEnumerable<Expression> Children { get; }

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

            public override Method ReplaceNodes(Func<Node?, Node?> _map)
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

            internal override ExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
            {
                ExpressionSyntax source;
                TypeArgumentListSyntax typeArgumentList = SyntaxFactoryHelper.TypeArguments(TypeArguments.Select(_a => _a.EmitNode(_scope, _tagger)));
                if (Source is NameExpression name)
                {
                    SyntaxToken identifier = name.Name.EmitToken(_tagger);
                    source = SyntaxFactory.GenericName(identifier, typeArgumentList);
                }
                else if (Source is MemberAccessExpression member)
                {

                    SyntaxToken identifier = member.Member.EmitToken(_tagger);
                    source = SyntaxFactoryHelper.MemberAccess(
                        member.Source.EmitNode(_scope, _tagger),
                        SyntaxFactory.GenericName(identifier, typeArgumentList));
                }
                else
                {
                    throw new InvalidOperationException("Not a method");
                }
                return source.Own(_tagger, this);
            }

            public override IEnumerable<NameOrMemberAccessExpression> Children => new[] { Source };
            public override string ToString() => LeftCode(Source, EPrecedence.Primary) + (IsGeneric ? $"<{string.Join(", ", TypeArguments)}>" : "");

        }

        public sealed record SimpleCallable(Expression Source) : Callable
        {

            public override SimpleCallable ReplaceNodes(Func<Node?, Node?> _map)
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

            internal override ExpressionSyntax EmitNode(Scope _scope, Tagger _tagger) => Source.EmitNode(_scope, _tagger);
            public override IEnumerable<Expression> Children => new[] { Source };
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

        public override InvocationExpression ReplaceNodes(Func<Node?, Node?> _map)
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

        internal override InvocationExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactoryHelper.Invocation(
                Source.EmitNode(_scope, _tagger),
                Arguments.Select(_a => _a.EmitNode(_scope, _tagger)))
            .Own(_tagger, this);
        public override IEnumerable<BodyNode> Children => new BodyNode[] { Source }.Concat(Arguments);
        public override string ToString() => $"{Source}({string.Join(", ", Arguments)})";

        #endregion

    }

}
