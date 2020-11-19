
using BS.Exceptions.Runtime.Expressions;
using BS.Runtime;

namespace BS.AST.Expressions
{

    public abstract class Expr : Syntax.IExpr
    {

        public abstract int Priority { get; }
        public abstract string Code { get; }

        internal abstract object Evaluate(Env _env);

        internal virtual void Assign(Env _env, object _value) => new UnassignableError(this);

    }

}
