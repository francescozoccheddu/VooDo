
namespace BS.AST.Expressions.Literals
{

    public sealed class BoolLitExpr : LitExpr<bool>
    {

        internal BoolLitExpr(bool _value) : base(_value)
        {
        }

        #region Expr

        public sealed override string Code => Syntax.FormatLitExp(Literal);

        #endregion

    }

}
