using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Operations
{

    public sealed class ProdOpExpr : BinaryOpExpr
    {

        internal ProdOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 3;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => Syntax.Symbols.prodOp;

        #endregion

    }

}
