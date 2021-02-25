
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record ObjectCreationExpression : InvocationOrObjectCreationExpression
    {

        
        public ObjectCreationExpression(ImmutableArray<Argument> _arguments) : this(null, _arguments) { }
        public ObjectCreationExpression(ComplexType? _type = null, ImmutableArray<Argument> _arguments = default)
        {
            Type = _type;
            Arguments = _arguments;
        }

        
        
        public ComplexType? Type { get; init; }
        private ImmutableArray<Argument> m_arguments;
        public ImmutableArray<Argument> Arguments
        {
            get => m_arguments;
            init => m_arguments = value.EmptyIfDefault();
        }
        public bool IsTypeImplicit => Type is null;

        
        
        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
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



        public override IEnumerable<Node> Children => IsTypeImplicit ? Arguments : new Node[] { Type! }.Concat(Arguments);
        public override string ToString() => $"{GrammarConstants.newKeyword} " + (IsTypeImplicit ? $"{Type} " : "") + $"({string.Join(", ", Arguments)})";

        
    }

}
