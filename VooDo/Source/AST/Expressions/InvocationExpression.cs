
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public record InvocationExpression(InvocationExpression.Callable Source, ImmutableArray<Argument> Arguments = default) : InvocationOrObjectCreationExpression
    {

        
        public abstract record Callable : BodyNode
        {


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

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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



            public override IEnumerable<Node> Children => new[] { Source };
            public override string ToString() => LeftCode(Source, EPrecedence.Primary) + (IsGeneric ? $"<{string.Join(", ", TypeArguments)}>" : "");

        }

        public sealed record SimpleCallable(Expression Source) : Callable
        {

            protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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

            public override IEnumerable<Node> Children => new[] { Source };
            public override string ToString() => LeftCode(Source, EPrecedence.Primary);

        }



        
        
        private ImmutableArray<Argument> m_arguments = Arguments.EmptyIfDefault();
        public ImmutableArray<Argument> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.EmptyIfDefault();
        }

        
        
        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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


        public override IEnumerable<Node> Children => new BodyNode[] { Source }.Concat(Arguments);
        public override string ToString() => $"{Source}({string.Join(", ", Arguments)})";

        
    }

}
