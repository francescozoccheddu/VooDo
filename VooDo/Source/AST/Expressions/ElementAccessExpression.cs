using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Compilation;
using VooDo.Compilation.Emission;
using VooDo.Errors.Problems;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ElementAccessExpression : AssignableExpression
    {

        #region Members

        public ElementAccessExpression(Expression _source, ImmutableArray<Expression> _arguments)
        {
            Source = _source;
            Arguments = _arguments;
        }

        public Expression Source { get; init; }

        private ImmutableArray<Expression> m_arguments;
        public ImmutableArray<Expression> Arguments
        {
            get => m_arguments;
            init
            {
                if (value.IsDefaultOrEmpty)
                {
                    throw new SyntaxError(this, "Element access expression must have at least one argument").AsThrowable();
                }
                m_arguments = value.EmptyIfDefault();
            }
        }

        #endregion

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        public override ElementAccessExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
        {
            Expression newSource = (Expression) _map(Source).NonNull();
            ImmutableArray<Expression> newArguments = Arguments.Map(_map).NonNull();
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

        internal override ElementAccessExpressionSyntax EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactory.ElementAccessExpression(
                Source.EmitNode(_scope, _tagger),
                SyntaxFactoryHelper.BracketedArguments(
                        Arguments.Select(_a => SyntaxFactory.Argument(_a.EmitNode(_scope, _tagger)).Own(_tagger, _a))))
            .Own(_tagger, this);
        public override IEnumerable<Expression> Children => new Expression[] { Source }.Concat(Arguments);
        public override string ToString() => $"{Source}[{string.Join(",", Arguments)}]";

        #endregion

    }

}
