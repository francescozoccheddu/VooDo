using VooDo.AST.Expressions;
using VooDo.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VooDo.AST.Expressions.Operators
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

        protected virtual bool m_SpaceBetweenOperator => true;

        #region ASTBase

        public sealed override string Code
            => LeftArgument.LeftCode(Precedence) + (m_SpaceBetweenOperator ? $" {m_OperatorSymbol} " : m_OperatorSymbol) + RightArgument.RightCode(Precedence);



        public override bool Equals(object _obj)
            => _obj is BinaryOpExpr expr && LeftArgument.Equals(expr.LeftArgument) && RightArgument.Equals(expr.RightArgument);

        public override int GetHashCode()
            => Identity.CombineHash(LeftArgument, RightArgument);

        #endregion

    }

}
