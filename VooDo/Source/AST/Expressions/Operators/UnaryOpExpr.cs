using VooDo.AST.Expressions;
using VooDo.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VooDo.AST.Expressions.Operators
{

    public abstract class UnaryOpExpr : Expr
    {

        internal UnaryOpExpr(Expr _argument)
        {
            Ensure.NonNull(_argument, nameof(_argument));
            Argument = _argument;
        }

        public Expr Argument { get; }

        protected abstract string m_OperatorSymbol { get; }

        #region ASTBase

        public sealed override string Code
            => $"{m_OperatorSymbol}{Argument.LeftCode(Priority)}";

        public sealed override bool Equals(object _obj)
            => _obj is UnaryOpExpr expr && Argument.Equals(expr.Argument);

        public sealed override int GetHashCode() => Argument.GetHashCode();

        #endregion

    }

}
