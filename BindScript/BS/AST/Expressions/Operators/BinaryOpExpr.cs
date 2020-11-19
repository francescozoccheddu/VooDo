using BS.AST.Expressions;
using BS.Exceptions;

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

    }

}
