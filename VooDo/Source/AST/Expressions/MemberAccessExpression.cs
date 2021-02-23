
using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record MemberAccessExpression(ComplexTypeOrExpression Source, Identifier Member) : NameOrMemberAccessExpression
    {

        
        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        protected internal override Node ReplaceNodes(Func<Node?, Node?> _map)
        {
            ComplexTypeOrExpression newSource = (ComplexTypeOrExpression) _map(Source).NonNull();
            Identifier newMember = (Identifier) _map(Member).NonNull();
            if (ReferenceEquals(newSource, Source) && ReferenceEquals(newMember, Member))
            {
                return this;
            }
            else
            {
                return this with
                {
                    Source = newSource,
                    Member = newMember
                };
            }
        }


        public override IEnumerable<Node> Children => new Node[] { Source, Member };
        public override string ToString() => $"{LeftCode(Source)}.{Member}";

        
    }

}
