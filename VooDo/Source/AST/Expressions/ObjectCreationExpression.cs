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

    public sealed record ObjectCreationExpression : InvocationOrObjectCreationExpression
    {

        #region Delegating constructors

        public ObjectCreationExpression(ImmutableArray<Argument> _arguments) : this(null, _arguments) { }
        public ObjectCreationExpression(ComplexType? _type = null, ImmutableArray<Argument> _arguments = default)
        {
            Type = _type;
            Arguments = _arguments;
        }

        #endregion

        #region Members

        public ComplexType? Type { get; init; }
        private ImmutableArray<Argument> m_arguments;
        public ImmutableArray<Argument> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.EmptyIfDefault();
        }
        public bool IsTypeImplicit => Type is null;

        #endregion

        #region Override

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        public override ObjectCreationExpression ReplaceNodes(Func<Node?, Node?> _map)
        {
            ComplexType? newType = (ComplexType?) _map(Type);
            ImmutableArray<Argument> newArguments = Arguments.Map(_map).NonNull();
            if (ReferenceEquals(newType, Type) && newArguments == Arguments)
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Arguments = newArguments
                };
            }
        }

        internal override ExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
        {
            ArgumentListSyntax argumentList = SyntaxFactoryUtils.Arguments(Arguments.Select(_a => _a.EmitNode(_scope, _tagger)));
            return (IsTypeImplicit
                ? (ExpressionSyntax) SyntaxFactory.ImplicitObjectCreationExpression(argumentList, null)
                : SyntaxFactory.ObjectCreationExpression(Type!.EmitNode(_scope, _tagger), argumentList, null))
                .Own(_tagger, this);
        }

        public override IEnumerable<BodyNode> Children => IsTypeImplicit ? Arguments : new BodyNode[] { Type! }.Concat(Arguments);
        public override string ToString() => $"{GrammarConstants.newKeyword} " + (IsTypeImplicit ? $"{Type} " : "") + $"({string.Join(", ", Arguments)})";

        #endregion

    }

}
