namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class SumOpExpr : BinaryOpExpr
    {

        internal SumOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        public sealed override int Precedence => 3;

        protected override object Evaluate(dynamic _left, dynamic _right) => _left + _right;

        #endregion

        #region Operator

        protected override Name m_OperatorMethod => "op_Addition";

        protected sealed override string m_OperatorSymbol => "+";

        #endregion

    }

}
