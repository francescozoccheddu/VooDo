
namespace VooDo.AST.Expressions.Literals
{

    public sealed class RealLitExpr : LitExpr<double>
    {

        internal RealLitExpr(double _value) : base(_value)
        {
        }

        #region Expr

        public sealed override string Code => Literal.ToString();

        #endregion

    }

}
