﻿namespace VooDo.AST.Expressions.Operators.Comparisons
{

    public sealed class EqOpExpr : BinaryOpExpr
    {

        internal EqOpExpr(Expr _leftArgument, Expr _rightArgument) : base(_leftArgument, _rightArgument)
        {
        }

        #region Expr

        protected sealed override object Evaluate(dynamic _left, dynamic _right) => _left == _right;

        public sealed override int Precedence => 6;

        #endregion

        #region Operator

        protected sealed override string m_OperatorSymbol => "==";

        #endregion

    }

}
