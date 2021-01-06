namespace VooDo.AST.Expressions.Operators.Comparisons
{

    public sealed class NeqOpExpr : BinaryOpExpr
    {

        internal NeqOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        protected sealed override object Evaluate(dynamic _left, dynamic _right) => _left != _right;

        public sealed override int Precedence => 6;

        #endregion

        protected override Name m_OperatorMethod => "op_Inequality";

        #region Operator

        protected sealed override string m_OperatorSymbol => "!=";

        #endregion

    }

}
