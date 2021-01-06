using VooDo.Exceptions.Runtime.Expressions;
using VooDo.Runtime;

namespace VooDo.AST.Expressions
{

    public abstract class Expr : ASTBase
    {

        internal abstract Eval Evaluate(Env _env);

        internal virtual void Assign(Env _env, Eval _value) => new UnassignableError(this);

        public abstract int Precedence { get; }

    }

}
