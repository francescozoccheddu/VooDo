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

    public sealed record ObjectCreationExpression : Expression
    {

        #region Delegating constructors

        public ObjectCreationExpression(ImmutableArray<InvocationExpression.Argument> _arguments) : this(null, _arguments) { }
        public ObjectCreationExpression(ComplexType? _type = null, ImmutableArray<InvocationExpression.Argument> _arguments = default)
        {
            Type = _type;
            Arguments = _arguments;
        }

        #endregion

        #region Members

        public ComplexType? Type { get; init; }
        private ImmutableArray<InvocationExpression.Argument> m_arguments;
        public ImmutableArray<InvocationExpression.Argument> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.EmptyIfDefault();
        }
        public bool IsTypeImplicit => Type is null;

        #endregion

        #region Override

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        public override ObjectCreationExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            ComplexType? newType = (ComplexType?) _map(Type);
            ImmutableArray<InvocationExpression.Argument> newArguments = Arguments.Map(_map).NonNull();
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

        internal override ExpressionSyntax EmitNode(Scope _scope, Marker _marker)
        {
            ArgumentListSyntax argumentList = SyntaxFactoryHelper.Arguments(Arguments.Select(_a => _a.EmitNode(_scope, _marker)));
            return (IsTypeImplicit
                ? (ExpressionSyntax) SyntaxFactory.ImplicitObjectCreationExpression(argumentList, null)
                : SyntaxFactory.ObjectCreationExpression(Type!.EmitNode(_scope, _marker), argumentList, null))
                .Own(_marker, this);
        }

        public override IEnumerable<Node> Children => IsTypeImplicit ? Arguments : new Node[] { Type! }.Concat(Arguments);
        public override string ToString() => $"{GrammarConstants.newKeyword} " + (IsTypeImplicit ? $"{Type} " : "") + $"({string.Join(", ", Arguments)})";

        #endregion

    }

}
