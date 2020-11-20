
namespace BS.AST.Expressions.Literals
{

    public sealed class RealLitExpr : LitExpr<double>
    {

        internal RealLitExpr(double _value) : base(_value)
        {
        }

        #region Expr

        public sealed override string Code => Syntax.FormatLitExp(Literal);

        #endregion

    }

}
