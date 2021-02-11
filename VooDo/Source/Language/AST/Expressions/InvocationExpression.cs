using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record InvocationExpression(InvocationExpression.Callable Source, ImmutableArray<InvocationExpression.Argument> Arguments = default) : Expression
    {

        #region Nested types

        public abstract record Callable : Node
        {

            internal abstract override ExpressionSyntax EmitNode(Scope _scope, Marker _marker);
            public abstract override IEnumerable<Expression> Children { get; }

        }

        public sealed record Method(NameOrMemberAccessExpression Source, ImmutableArray<ComplexType> TypeArguments = default) : Callable
        {

            private static bool IsValidSource(NameOrMemberAccessExpression _source)
                => _source is MemberAccessExpression || (_source is NameExpression name && !name.IsControllerOf);


            private NameOrMemberAccessExpression m_source = Source.Assert(IsValidSource);
            public NameOrMemberAccessExpression Source
            {
                get => m_source;
                init => m_source = value.Assert(IsValidSource);
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
            public override string ToString() => Source.ToString();

        }

        public sealed record SimpleCallable(Expression Source) : Callable
        {

            internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker) => Source.EmitNode(_scope, _marker);
            public override IEnumerable<Expression> Children => new[] { Source };
            public override string ToString() => Source.ToString();

        }

        public abstract record Argument : Node
        {

            public enum EKind
            {
                Value, Ref, Out
            }

            internal Argument() { }

            public abstract EKind Kind { get; }

            internal abstract override ArgumentSyntax EmitNode(Scope _scope, Marker _marker);

        }

        public sealed record ValueArgument(Expression Expression) : Argument
        {
            public override EKind Kind => EKind.Value;
            internal override ArgumentSyntax EmitNode(Scope _scope, Marker _marker)
                => SyntaxFactory.Argument(Expression.EmitNode(_scope, _marker)).Own(_marker, this);
            public override IEnumerable<Expression> Children => new[] { Expression };
            public override string ToString() => $"{Kind.Token()} {Expression}".TrimStart();
        }

        public sealed record AssignableArgument(Argument.EKind AssignableKind, AssignableExpression Expression) : Argument
        {
            public override EKind Kind => AssignableKind;
            internal override ArgumentSyntax EmitNode(Scope _scope, Marker _marker)
                => SyntaxFactory.Argument(Expression.EmitNode(_scope, _marker))
                .WithRefKindKeyword(SyntaxFactory.Token(AssignableKind switch
                {
                    EKind.Value => SyntaxKind.None,
                    EKind.Ref => SyntaxKind.RefKeyword,
                    EKind.Out => SyntaxKind.OutKeyword,
                    _ => throw new InvalidOperationException(),
                }))
                .Own(_marker, this);
            public override IEnumerable<AssignableExpression> Children => new[] { Expression };
            public override string ToString() => $"{Kind.Token()} {Expression}".TrimStart();
        }

        public sealed record OutDeclarationArgument(ComplexTypeOrVar Type, IdentifierOrDiscard Name) : Argument
        {
            public override EKind Kind => EKind.Out;
            internal override ArgumentSyntax EmitNode(Scope _scope, Marker _marker)
                => SyntaxFactory.Argument(
                    SyntaxFactory.DeclarationExpression(
                        Type.EmitNode(_scope, _marker),
                        Name.EmitNode(_scope, _marker)))
                .WithRefKindKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword))
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
