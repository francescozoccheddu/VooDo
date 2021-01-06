namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class NegOpExpr : UnaryOpExpr
    {

        internal NegOpExpr(Expr _argument) : base(_argument)
        {
        }

        #region Expr

        public sealed override int Precedence => 1;

        #endregion

        #region Operator

        protected override Name m_OperatorMethod => "op_UnaryNegation";

        protected sealed override string m_OperatorSymbol => "-";

        protected override object Evaluate(dynamic _argument) => -_argument;

        #endregion

    }

}
