
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.Problems;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ElementAccessExpression : AssignableExpression
    {

        
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

        
        
        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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


        public override IEnumerable<Node> Children => new Node[] { Source }.Concat(Arguments);
        public override string ToString() => $"{Source}[{string.Join(",", Arguments)}]";

        
    }

}
