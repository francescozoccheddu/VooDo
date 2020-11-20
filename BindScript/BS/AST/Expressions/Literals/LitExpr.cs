using BS.Exceptions.Runtime.Expressions;
using BS.Runtime;

namespace BS.AST.Expressions.Literals
{

    public abstract class LitExpr<T> : Expr
    {

        internal LitExpr(T _value)
        {
            Literal = _value;
        }

        public T Literal { get; }

        public sealed override int Priority => 0;

        internal sealed override object Evaluate(Env _env) => Literal;

    }

}
