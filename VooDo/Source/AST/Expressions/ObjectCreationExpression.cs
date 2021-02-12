using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ObjectCreationExpression(ComplexType? Type, ImmutableArray<InvocationExpression.Argument> Arguments = default) : Expression
    {

        #region Delegating constructors

        public ObjectCreationExpression(params InvocationExpression.Argument[] _arguments) : this(_arguments.ToImmutableArray()) { }
        public ObjectCreationExpression(ComplexType? _type, params InvocationExpression.Argument[] _arguments) : this(_type, _arguments.EmptyIfNull().ToImmutableArray()) { }
        public ObjectCreationExpression(IEnumerable<InvocationExpression.Argument>? _arguments) : this(_arguments.EmptyIfNull().ToImmutableArray()) { }
        public ObjectCreationExpression(ComplexType? _type, IEnumerable<InvocationExpression.Argument>? _arguments) : this(_type, _arguments.EmptyIfNull().ToImmutableArray()) { }
        public ObjectCreationExpression(ImmutableArray<InvocationExpression.Argument> _arguments) : this(null, _arguments) { }

        #endregion

        #region Members

        private ImmutableArray<InvocationExpression.Argument> m_arguments = Arguments.EmptyIfDefault();
        public ImmutableArray<InvocationExpression.Argument> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.EmptyIfDefault();
        }
        public bool IsTypeImplicit => Type is null;

        #endregion

        #region Override

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
