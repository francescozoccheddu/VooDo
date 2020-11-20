using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Comparisons
{

    public sealed class LeOpExpr : BinaryOpExpr
    {

        internal LeOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 5;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => Syntax.Symbols.leOp;

        #endregion

    }

}
