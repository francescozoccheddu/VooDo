namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class LogOrOpExpr : BinaryOpExpr
    {

        internal LogOrOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        protected override object Evaluate(dynamic _left, dynamic _right) => _left || _right;

        public sealed override int Precedence => 11;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => "||";

        protected override Name m_OperatorMethod => "op_LogicalOr";

        #endregion

    }

}
