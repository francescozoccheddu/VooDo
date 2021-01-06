namespace VooDo.AST.Expressions.Operators.Comparisons
{

    public sealed class EqOpExpr : BinaryOpExpr
    {

        internal EqOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        public sealed override int Precedence => 6;

        #endregion

        #region Operator

        protected override object Evaluate(dynamic _left, dynamic _right) => _left == _right;

        protected override Name m_OperatorMethod => "op_Equality";

        protected sealed override string m_OperatorSymbol => "==";

        #endregion

    }

}
