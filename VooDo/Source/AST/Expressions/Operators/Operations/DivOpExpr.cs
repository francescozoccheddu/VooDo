﻿namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class DivOpExpr : BinaryOpExpr
    {

        internal DivOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        protected override object Evaluate(dynamic _left, dynamic _right) => _left / _right;

        public sealed override int Precedence => 2;

        #endregion

        #region Operator

        protected override Name m_OperatorMethod => "op_Division";

        protected sealed override string m_OperatorSymbol => "/";

        #endregion

    }

}
