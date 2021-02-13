using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Compilation;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record MemberAccessExpression(ComplexTypeOrExpression Source, Identifier Member) : NameOrMemberAccessExpression
    {

        #region Overrides

        protected override EPrecedence m_Precedence => EPrecedence.Primary;

        public override MemberAccessExpression ReplaceNodes(Func<NodeOrIdentifier?, NodeOrIdentifier?> _map)
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

        internal override MemberAccessExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactoryHelper.MemberAccess(
                Source.EmitNode(_scope, _marker),
                Member.EmitToken(_marker).Own(_marker, Member))
            .Own(_marker, this);
        public override IEnumerable<NodeOrIdentifier> Children => new NodeOrIdentifier[] { Source, Member };
        public override string ToString() => $"{LeftCode(Source)}.{Member}";

        #endregion

    }

}
