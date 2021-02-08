
using System.Collections.Generic;

using VooDo.Language.AST.Names;
using VooDo.Language.AST.Statements;

namespace VooDo.Language.AST.Directives
{

    public sealed record UsingNamespaceDirective(Identifier? Alias, Namespace Namespace) : UsingDirective
    {

        #region Delegating constructors



        #endregion

        #region Members

        public bool HasAlias => Alias is not null;

        #endregion

        #region Overrides

        public override IEnumerable<Node> Children => HasAlias ? new Node[] { Alias!, Namespace } : new Node[] { Namespace };
        public override string ToString() => $"{GrammarConstants.usingKeyword} "
            + (HasAlias ? $"{Alias} {AssignmentStatement.EKind.Simple.Token()} " : "")
            + Namespace
            + GrammarConstants.statementEndToken;

        #endregion

    }

}
