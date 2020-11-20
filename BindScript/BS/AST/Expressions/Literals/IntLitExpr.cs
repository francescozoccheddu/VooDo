
namespace BS.AST.Expressions.Literals
{

    public sealed class IntLitExpr : LitExpr<int>
    {

        internal IntLitExpr(int _value) : base(_value)
        {
        }

        public sealed override string Code => Syntax.FormatLitExp(Literal);
    }

}
