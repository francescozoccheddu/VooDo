namespace VooDo.Language.AST.Expressions
{

    public sealed record GlobalExpression(Expression ControllerExpression, Expression? InitializerExpression) : Expression
    {

        #region Members

        public bool HasInitializer => InitializerExpression is not null;

        #endregion

        #region Overrides

        public override string ToString() => $"{GrammarConstants.globKeyword} {ControllerExpression}" + (HasInitializer ? $" {GrammarConstants.initKeyword} {InitializerExpression}" : "");

        #endregion


    }

}
