using System;

using VooDo.Runtime;

namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class SumOpExpr : BinaryOpExpr
    {

        internal SumOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        internal sealed override object Evaluate(Runtime.Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Precedence => 3;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => "+";

        #endregion

    }

}
