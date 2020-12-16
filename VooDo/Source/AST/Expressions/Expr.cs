
using VooDo.AST.Expressions.Fundamentals;
using VooDo.AST.Expressions.Literals;
using VooDo.AST.Expressions.Operators.Comparisons;
using VooDo.AST.Expressions.Operators.Operations;
using VooDo.Exceptions.Runtime.Expressions;
using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions
{

    public abstract class Expr : ASTBase
    {

        internal abstract object Evaluate(Env _env);

        internal virtual void Assign(Env _env, object _value) => new UnassignableError(this);

        public abstract int Priority { get; }

    }

}
