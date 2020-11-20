using BS.AST.Expressions;
using BS.Exceptions;
using BS.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BS.AST.Expressions.Operators
{

    public abstract class BinaryOpExpr : Expr
    {

        internal BinaryOpExpr(Expr _leftArgument, Expr _rightArgument)
        {
            Ensure.NonNull(_leftArgument, nameof(_leftArgument));
            Ensure.NonNull(_rightArgument, nameof(_rightArgument));
            LeftArgument = _leftArgument;
            RightArgument = _rightArgument;
        }

        public Expr LeftArgument { get; }
        public Expr RightArgument { get; }

        protected abstract string m_OperatorSymbol { get; }

        #region ASTBase

        public sealed override string Code
            => Syntax.FormatBinaryOp
            (LeftArgument.Priority > Priority ? Syntax.WrapExp(LeftArgument.Code) : LeftArgument.Code,
            RightArgument.Priority >= Priority ? Syntax.WrapExp(RightArgument.Code) : RightArgument.Code,
                m_OperatorSymbol);


        public sealed override bool Equals(object _obj)
            => _obj is BinaryOpExpr expr && LeftArgument.Equals(expr.LeftArgument) && RightArgument.Equals(expr.RightArgument);

        public sealed override int GetHashCode()
            => Hash.Combine(LeftArgument, RightArgument);

        #endregion

    }

}
