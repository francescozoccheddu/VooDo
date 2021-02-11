using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Language.AST.Names;
using VooDo.Language.Linking;
using VooDo.Utils;

namespace VooDo.Language.AST.Expressions
{

    public sealed record MemberAccessExpression(ComplexTypeOrExpression Source, Identifier Member) : NameOrMemberAccessExpression
    {

        #region Overrides

        internal override MemberAccessExpressionSyntax EmitNode(Scope _scope, Marker _marker)
            => SyntaxFactoryHelper.MemberAccess(
                Source.EmitNode(_scope, _marker),
                SyntaxFactory.IdentifierName(Member.EmitToken(_marker)).Own(_marker, Member))
            .Own(_marker, this);
        public override IEnumerable<NodeOrIdentifier> Children => new NodeOrIdentifier[] { Source, Member };
        public override string ToString() => $"{Source}.{Member}";

        #endregion

    }

}
