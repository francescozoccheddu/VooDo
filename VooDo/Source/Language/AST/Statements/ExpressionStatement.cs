using System.Collections.Generic;

using VooDo.Language.AST.Expressions;

namespace VooDo.Language.AST.Statements
{

    public sealed record ExpressionStatement(Expression Expression) : Statement
    {

        #region Overrides

        public override IEnumerable<Node> Children => new Node[] { Expression };
        public override string ToString() => $"{Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
