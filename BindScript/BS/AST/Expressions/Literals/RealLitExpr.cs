
namespace BS.AST.Expressions.Literals
{

    public sealed class RealLitExpr : LitExpr<double>
    {

        internal RealLitExpr(double _value) : base(_value)
        {
        }

        public sealed override string Code => Syntax.FormatLitExp(Literal);
    }

}
