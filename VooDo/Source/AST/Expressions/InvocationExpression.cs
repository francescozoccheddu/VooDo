using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record InvocationExpression(InvocationExpression.Callable Source, ImmutableArray<InvocationExpression.Argument> Arguments = default) : Expression
    {

        #region Nested types

        public abstract record Callable : Node
        {

            public abstract override Callable ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);
            internal abstract override ExpressionSyntax EmitNode(Scope _scope, Marker _marker);
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

            public override Method ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

            internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            {
                ExpressionSyntax source;
                TypeArgumentListSyntax typeArgumentList = SyntaxFactoryHelper.TypeArguments(TypeArguments.Select(_a => _a.EmitNode(_scope, _marker)));
                if (Source is NameExpression name)
                {
                    SyntaxToken identifier = name.Name.EmitToken(_marker);
                    source = SyntaxFactory.GenericName(identifier, typeArgumentList);
                }
                else if (Source is MemberAccessExpression member)
                {

                    SyntaxToken identifier = member.Member.EmitToken(_marker);
                    source = SyntaxFactoryHelper.MemberAccess(
                        member.Source.EmitNode(_scope, _marker),
                        SyntaxFactory.GenericName(identifier, typeArgumentList));
                }
                else
                {
                    throw new InvalidOperationException("Not a method");
                }
                return source.Own(_marker, this);
            }

            public override IEnumerable<NameOrMemberAccessExpression> Children => new[] { Source };
            public override string ToString() => LeftCode(Source, EPrecedence.Primary) + (IsGeneric ? $"<{string.Join(", ", TypeArguments)}>" : "");

        }

        public sealed record SimpleCallable(Expression Source) : Callable
        {

            public override SimpleCallable ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

            internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker) => Source.EmitNode(_scope, _marker);
            public override IEnumerable<Expression> Children => new[] { Source };
            public override string ToString() => LeftCode(Source, EPrecedence.Primary);

        }

        public abstract record Argument(Identifier? Parameter) : Node
        {

            public enum EKind
            {
                Value, Ref, Out, In
            }

            public abstract EKind Kind { get; }

            protected abstract Argument ReplaceArgumentNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map);

            public sealed override Argument ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
            {
                Identifier? newParameter = (Identifier?) _map(Parameter);
                if (ReferenceEquals(newParameter, Parameter))
                {
                    return ReplaceArgumentNodes(_map);
                }
                else
                {
                    return ReplaceArgumentNodes(_map) with
                    {
                        Parameter = newParameter
                    };
                }
            }


            private protected abstract ExpressionSyntax EmitArgumentExpression(Scope _scope, Marker _marker);
            internal sealed override ArgumentSyntax EmitNode(Scope _scope, Marker _marker)
                => SyntaxFactory.Argument(EmitArgumentExpression(_scope, _marker))
                    .WithRefKindKeyword(SyntaxFactory.Token(Kind switch
                    {
                        EKind.Value => SyntaxKind.None,
                        EKind.Ref => SyntaxKind.RefKeyword,
                        EKind.Out => SyntaxKind.OutKeyword,
                        EKind.In => SyntaxKind.InKeyword,
                        _ => throw new InvalidOperationException(),
                    }))
                .Own(_marker, this);

        }

        public sealed record ValueArgument(Identifier? Parameter, Expression Expression) : Argument(Parameter)
        {
            public override EKind Kind => EKind.Value;

            protected override ValueArgument ReplaceArgumentNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
            {
                Expression newExpression = (Expression) _map(Expression).NonNull();
                if (ReferenceEquals(newExpression, Expression))
                {
                    return this;
                }
                else
                {
                    return this with
                    {
                        Expression = newExpression
                    };
                }
            }

            private protected override ExpressionSyntax EmitArgumentExpression(Scope _scope, Marker _marker)
                => Expression.EmitNode(_scope, _marker).Own(_marker, this);
            public override IEnumerable<Expression> Children => new[] { Expression };
            public override string ToString() => $"{Kind.Token()} {Expression}".TrimStart();
        }

        public sealed record AssignableArgument(Identifier? Parameter, Argument.EKind AssignableKind, AssignableExpression Expression) : Argument(Parameter)
        {
            public override EKind Kind => AssignableKind;
            protected override AssignableArgument ReplaceArgumentNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
            {
                AssignableExpression newExpression = (AssignableExpression) _map(Expression).NonNull();
                if (ReferenceEquals(newExpression, Expression))
                {
                    return this;
                }
                else
                {
                    return this with
                    {
                        Expression = newExpression
                    };
                }
            }
            private protected override ExpressionSyntax EmitArgumentExpression(Scope _scope, Marker _marker)
                => Expression.EmitNode(_scope, _marker).Own(_marker, this);
            public override IEnumerable<AssignableExpression> Children => new[] { Expression };
            public override string ToString() => $"{Kind.Token()} {Expression}".TrimStart();
        }

        public sealed record OutDeclarationArgument(Identifier? Parameter, ComplexTypeOrVar Type, IdentifierOrDiscard Name) : Argument(Parameter)
        {
            public override EKind Kind => EKind.Out;
            protected override OutDeclarationArgument ReplaceArgumentNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
            {
                ComplexTypeOrVar newType = (ComplexTypeOrVar) _map(Type).NonNull();
                IdentifierOrDiscard newName = (IdentifierOrDiscard) _map(Parameter).NonNull();
                if (ReferenceEquals(newType, Type) && ReferenceEquals(newName, Name))
                {
                    return this;
                }
                else
                {
                    return this with
                    {
                        Type = newType,
                        Name = newName
                    };
                }
            }
            private protected override ExpressionSyntax EmitArgumentExpression(Scope _scope, Marker _marker)
                => SyntaxFactory.DeclarationExpression(
                        Type.EmitNode(_scope, _marker),
                        Name.EmitNode(_scope, _marker))
                .Own(_marker, this);
            public override IEnumerable<NodeOrIdentifier> Children => new NodeOrIdentifier[] { Type, Name };
            public override string ToString() => $"{Kind.Token()} {Type} {Name}".TrimStart();
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

        public override InvocationExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

        internal override InvocationExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactoryHelper.Invocation(
                Source.EmitNode(_scope, _marker),
                Arguments.Select(_a => _a.EmitNode(_scope, _marker)))
            .Own(_marker, this);
        public override IEnumerable<Node> Children => new Node[] { Source }.Concat(Arguments);
        public override string ToString() => $"{Source}({string.Join(", ", Arguments)})";

        #endregion

    }

}
