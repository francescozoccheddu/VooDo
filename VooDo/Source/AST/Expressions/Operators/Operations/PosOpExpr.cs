using System;

using VooDo.Runtime;

namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class PosOpExpr : UnaryOpExpr
    {

        internal PosOpExpr(Expr _argument) : base(_argument)
        {
        }

        #region Expr

        internal sealed override object Evaluate(Runtime.Env _env)
        {
            throw new NotImplementedException();
        }

        public sealed override int Precedence => 1;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => "+";

        #endregion

    }

}
