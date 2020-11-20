
namespace BS.AST.Expressions.Literals
{

    public sealed class NullLitExpr : LitExpr<object>
    {

        internal NullLitExpr() : base(null)
        {
        }

        public sealed override string Code => Syntax.Symbols.nullLit;
    }

}
