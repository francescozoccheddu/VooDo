using VooDo.Language.AST.Expressions;

namespace VooDo.Language.AST.Statements
{

    public sealed record ExpressionStatement(Expression Expression)
    {

        #region Overrides

        public override string ToString() => $"{Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
