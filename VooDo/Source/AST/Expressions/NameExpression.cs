
using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record NameExpression(bool IsControllerOf, Identifier Name) : NameOrMemberAccessExpression
    {

        
        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            Identifier newName = (Identifier) _map(Name).NonNull();
            if (ReferenceEquals(newName, Name))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Name = newName
                };
            }
        }



        public override IEnumerable<Node> Children => new Node[] { Name };
        public override string ToString() => (IsControllerOf ? "$" : "") + $"{Name}";

        
    }

}
