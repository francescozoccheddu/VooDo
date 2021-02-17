using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;

using VooDo.AST.Names;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public sealed record MemberAccessExpression(ComplexTypeOrExpression Source, Identifier Member) : NameOrMemberAccessExpression
    {

        #region Overrides

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

        internal override SyntaxNode EmitNode(Scope _scope, Tagger _tagger)
            => SyntaxFactoryUtils.MemberAccess(
                (ExpressionSyntax) Source.EmitNode(_scope, _tagger),
                Member.EmitToken(_tagger).Own(_tagger, Member))
            .Own(_tagger, this);
        public override IEnumerable<Node> Children => new Node[] { Source, Member };
        public override string ToString() => $"{LeftCode(Source)}.{Member}";

        #endregion

    }

}
