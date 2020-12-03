using BS.Exceptions.Runtime.Expressions;
using BS.Runtime;
using BS.Utils;

using System.Collections.Generic;

namespace BS.AST.Expressions.Literals
{

    public abstract class LitExpr<T> : Expr
    {

        internal LitExpr(T _value)
        {
            Literal = _value;
        }

        public T Literal { get; }

        #region Expr

        internal sealed override object Evaluate(Env _env) => Literal;

        public sealed override int Priority => 0;

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj) => _obj is LitExpr<T> expr && Literal.Equals(expr.Literal);

        public sealed override int GetHashCode() => Identity.CombineHash(Literal);

        #endregion

    }

}
