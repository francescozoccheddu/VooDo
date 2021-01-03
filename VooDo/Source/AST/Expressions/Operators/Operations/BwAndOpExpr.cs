namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class BwAndOpExpr : BinaryOpExpr
    {

        internal BwAndOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        protected override object Evaluate(dynamic _left, dynamic _right) => _left & _right;

        public sealed override int Precedence => 7;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => "&";

        #endregion

    }

}
