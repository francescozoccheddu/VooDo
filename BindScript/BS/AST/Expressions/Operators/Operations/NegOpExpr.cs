using System;

using BS.Runtime;

namespace BS.AST.Expressions.Operators.Operations
{

    public sealed class NegOpExpr : UnaryOpExpr
    {

        internal NegOpExpr(Expr _argument) : base(_argument)
        {
        }

        #region Expr

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 1;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => Syntax.Symbols.negOp;

        #endregion

    }

}
