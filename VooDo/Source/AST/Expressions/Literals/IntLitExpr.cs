
namespace VooDo.AST.Expressions.Literals
{

    public sealed class IntLitExpr : LitExpr<int>
    {

        internal IntLitExpr(int _value) : base(_value)
        {
        }

        #region Expr

        public sealed override string Code => Literal.ToString();

        #endregion

    }

}
