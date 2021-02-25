
using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Names;

namespace VooDo.AST.Expressions
{

    public sealed record DefaultExpression(ComplexType? Type = null) : Expression
    {

        
        public bool HasType => Type is not null;

        
        
        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ComplexType? newType = (ComplexType?) _map(Type);
            if (ReferenceEquals(newType, Type))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType
                };
            }
        }


        public override IEnumerable<Node> Children => HasType ? new ComplexType[] { Type! } : Enumerable.Empty<ComplexType>();
        public override string ToString() => GrammarConstants.defaultKeyword + (HasType ? $"({Type})" : "");

        
    }

}
