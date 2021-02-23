
using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST.Names;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record IsExpression(Expression Expression, ComplexType Type, IdentifierOrDiscard? Name = null) : Expression
    {

        
        public bool IsDeclaration => Name is not null;

        
        
        protected override EPrecedence m_Precedence => EPrecedence.Relational;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ComplexType newType = (ComplexType) _map(Type).NonNull();
            IdentifierOrDiscard? newName = (IdentifierOrDiscard?) _map(Name);
            if (ReferenceEquals(newType, Type) && ReferenceEquals(newName, Name))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Type = newType,
                    Name = newName
                };
            }
        }


        public override IEnumerable<Node> Children
            => new Node[] { Expression, Type }.Concat(IsDeclaration ? new Node[] { Name! } : Enumerable.Empty<Node>());
        public override string ToString() => $"{LeftCode(Expression)} {GrammarConstants.isKeyword} {Type}" + (IsDeclaration ? $" {Name}" : "");

        
    }

}
