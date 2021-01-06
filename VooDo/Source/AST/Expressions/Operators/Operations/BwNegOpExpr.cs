﻿namespace VooDo.AST.Expressions.Operators.Operations
{

    public sealed class BwNegOpExpr : UnaryOpExpr
    {

        internal BwNegOpExpr(Expr _argument) : base(_argument)
        {
        }

        #region Expr

        protected override object Evaluate(dynamic _argument) => ~_argument;

        public sealed override int Precedence => 1;

        #endregion

        #region Operator

        protected override Name m_OperatorMethod => "op_OnesComplement";

        protected sealed override string m_OperatorSymbol => "~";

        #endregion

    }

}
