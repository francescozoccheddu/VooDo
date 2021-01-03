using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime.Engine;
using VooDo.Runtime.Meta;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class CallExpr : ParametricExpr
    {

        internal CallExpr(Expr _callable, IEnumerable<Expr> _arguments) : base(_callable, _arguments)
        {
        }

        #region Expr

        internal sealed override object Evaluate(Runtime.Env _env)
            => RuntimeHelpers.Cast<ICallable>(Source.Evaluate(_env)).Call(Arguments.Select(_a => _a.Evaluate(_env)).ToArray());

        public sealed override int Precedence => 0;

        public sealed override string Code =>
               $"{Source.LeftCode(Precedence)}({Arguments.ArgumentsListCode()})";

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj) => _obj is CallExpr && base.Equals(_obj);

        public override int GetHashCode() => base.GetHashCode();

        #endregion

    }

}
