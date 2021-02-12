using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ElementAccessExpression(Expression Source, ImmutableArray<Expression> Arguments) : AssignableExpression
    {

        #region Delegating constructors

        public ElementAccessExpression(Expression _source, params Expression[] _arguments) : this(_source, _arguments.ToImmutableArray()) { }
        public ElementAccessExpression(Expression _source, IEnumerable<Expression> _arguments) : this(_source, _arguments.ToImmutableArray()) { }

        #endregion

        #region Members

        private ImmutableArray<Expression> m_arguments = Arguments.NonEmpty();
        public ImmutableArray<Expression> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.NonEmpty();
        }

        #endregion

        #region Overrides

        internal override ElementAccessExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactory.ElementAccessExpression(
                Source.EmitNode(_scope, _marker),
                SyntaxFactoryHelper.BracketedArguments(
                        Arguments.Select(_a => SyntaxFactory.Argument(_a.EmitNode(_scope, _marker)).Own(_marker, _a))))
            .Own(_marker, this);
        public override IEnumerable<Expression> Children => new Expression[] { Source }.Concat(Arguments);
        public override string ToString() => $"{Source}[{string.Join(",", Arguments)}]";

        #endregion

    }

}
