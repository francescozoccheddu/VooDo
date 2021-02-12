
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Generic;

using VooDo.Compilation;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Compilation;

namespace VooDo.AST.Directives
{

    public sealed record UsingNamespaceDirective(Identifier? Alias, Namespace Namespace) : UsingDirective
    {

        #region Delegating constructors



        #endregion

        #region Members

        public bool HasAlias => Alias is not null;

        #endregion

        #region Overrides

        internal override UsingDirectiveSyntax EmitNode(Scope _scope, Marker _marker)
        {
            NameSyntax name = Namespace.EmitNode(_scope, _marker);
            UsingDirectiveSyntax result;
            if (HasAlias)
            {
                SyntaxToken alias = Alias!.EmitToken(_marker);
                NameEqualsSyntax aliasName = SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(alias)).Own(_marker, Alias);
                result = SyntaxFactory.UsingDirective(aliasName, name);
            }
            else
            {
                result = SyntaxFactory.UsingDirective(name);
            }
            return result.Own(_marker, this);
        }

        public override IEnumerable<NodeOrIdentifier> Children => HasAlias ? new NodeOrIdentifier[] { Alias!, Namespace } : new NodeOrIdentifier[] { Namespace };

        public override string ToString() => $"{GrammarConstants.usingKeyword} "
            + (HasAlias ? $"{Alias} {AssignmentStatement.EKind.Simple.Token()} " : "")
            + Namespace
            + GrammarConstants.statementEndToken;

        #endregion

    }

}
