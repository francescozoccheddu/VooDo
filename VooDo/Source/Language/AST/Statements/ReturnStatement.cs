using VooDo.Language.AST.Expressions;

namespace VooDo.Language.AST.Statements
{

    public sealed record ReturnStatement(Expression Expression) : Statement
    {

        #region Overrides

        public override string ToString() => $"{GrammarConstants.returnKeyword} {Expression}{GrammarConstants.statementEndToken}";

        #endregion

    }

}
