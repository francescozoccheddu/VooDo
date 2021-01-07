
using VooDo.Runtime;

namespace VooDo.AST.Expressions.Literals
{

    public sealed class NullLitExpr : LitExpr<object>
    {

        internal NullLitExpr() : base(null)
        {
        }

        #region Expr

        public sealed override string Code => "null";

        internal override Eval Evaluate(Env _env) => new Eval(null);

        #endregion

    }

}
