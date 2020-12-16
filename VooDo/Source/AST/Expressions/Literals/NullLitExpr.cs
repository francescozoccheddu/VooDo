
namespace VooDo.AST.Expressions.Literals
{

    public sealed class NullLitExpr : LitExpr<object>
    {

        internal NullLitExpr() : base(null)
        {
        }

        #region Expr

        public sealed override string Code => "null";

        #endregion

    }

}
