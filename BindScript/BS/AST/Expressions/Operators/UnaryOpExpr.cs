using BS.AST.Expressions;
using BS.Exceptions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BS.AST.Expressions.Operators
{

    public abstract class UnaryOpExpr : Expr
    {

        internal UnaryOpExpr(Expr _argument)
        {
            Ensure.NonNull(_argument, nameof(_argument));
            Argument = _argument;
        }

        public Expr Argument { get; }

    }

}
