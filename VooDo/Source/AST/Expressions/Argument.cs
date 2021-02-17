using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public abstract record Argument(Identifier? Parameter) : BodyNode
    {

        public enum EKind
        {
            Value, Ref, Out, In
        }

        public abstract EKind Kind { get; }

        protected abstract Argument ReplaceArgumentNodes(Func<Node?, Node?> _map);

#if NET5_0
        public override InvocationOrObjectCreationExpression? Parent => (InvocationOrObjectCreationExpression?) base.Parent;
#endif

        protected internal sealed override Node ReplaceNodes(Func<Node?, Node?> _map)
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


        private protected abstract ExpressionSyntax EmitArgumentExpression(Scope _scope, Tagger _tagger);

        internal sealed override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.Argument(EmitArgumentExpression(_scope, _tagger))
                .WithRefKindKeyword(SyntaxFactory.Token(Kind switch
                {
                    EKind.Value => SyntaxKind.None,
                    EKind.Ref => SyntaxKind.RefKeyword,
                    EKind.Out => SyntaxKind.OutKeyword,
                    EKind.In => SyntaxKind.InKeyword
                }))
                .WithNameColon(Parameter is null
                    ? null
                    : SyntaxFactory.NameColon(
                        SyntaxFactory.IdentifierName(Parameter.EmitToken(_tagger))))
                .Own(_tagger, this);

    }

    public sealed record ValueArgument(Identifier? Parameter, Expression Expression) : Argument(Parameter)
    {
        public override EKind Kind => EKind.Value;

        protected override Argument ReplaceArgumentNodes(Func<Node?, Node?> _map)
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

        private protected override ExpressionSyntax EmitArgumentExpression(Scope _scope, Tagger _tagger)
            => (ExpressionSyntax) Expression.EmitNode(_scope, _tagger).Own(_tagger, this);
        public override IEnumerable<Node> Children => new[] { Expression };
        public override string ToString() => $"{Kind.Token()} {Expression}".TrimStart();
    }

    public sealed record AssignableArgument(Identifier? Parameter, Argument.EKind AssignableKind, AssignableExpression Expression) : Argument(Parameter)
    {
        public override EKind Kind => AssignableKind;
        protected override Argument ReplaceArgumentNodes(Func<Node?, Node?> _map)
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
        private protected override ExpressionSyntax EmitArgumentExpression(Scope _scope, Tagger _tagger)
            => (ExpressionSyntax) Expression.EmitNode(_scope, _tagger).Own(_tagger, this);
        public override IEnumerable<Node> Children => new[] { Expression };
        public override string ToString() => $"{Kind.Token()} {Expression}".TrimStart();
    }

    public sealed record OutDeclarationArgument(Identifier? Parameter, ComplexTypeOrVar Type, IdentifierOrDiscard Name) : Argument(Parameter)
    {
        public override EKind Kind => EKind.Out;
        protected override Argument ReplaceArgumentNodes(Func<Node?, Node?> _map)
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
        private protected override ExpressionSyntax EmitArgumentExpression(Scope _scope, Tagger _tagger)
            => SyntaxFactory.DeclarationExpression(
                    (TypeSyntax) Type.EmitNode(_scope, _tagger),
                    (VariableDesignationSyntax) Name.EmitNode(_scope, _tagger))
            .Own(_tagger, this);
        public override IEnumerable<Node> Children => new Node[] { Type, Name };
        public override string ToString() => $"{Kind.Token()} {Type} {Name}".TrimStart();
    }

}
