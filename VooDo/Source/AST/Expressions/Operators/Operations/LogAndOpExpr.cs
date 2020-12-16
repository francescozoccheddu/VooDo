using System;

using VooDo.Runtime;

namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class LogAndOpExpr : BinaryOpExpr
    {

        internal LogAndOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        internal sealed override object Evaluate(Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Priority => 7;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => "&&";

        #endregion

    }

}
