using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime;
using VooDo.Runtime.Meta;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class CallExpr : ParametricExpr
    {

        internal CallExpr(Expr _callable, IEnumerable<Expr> _arguments, bool _nullCoalesce) : base(_callable, _arguments) => NullCoalesce = _nullCoalesce;

        public bool NullCoalesce { get; }

        #region Expr

        internal sealed override Eval Evaluate(Env _env)
        {
            ICallable callable = Reflection.Cast<ICallable>(Source.Evaluate(_env));
            if (callable == null && NullCoalesce)
            {
                return new Eval(null);
            }
            else
            {
                return callable.Call(_env, Arguments.Select(_a => _a.Evaluate(_env)).ToArray());
            }
        }

        public sealed override int Precedence => 0;

        public sealed override string Code =>
               $"{Source.LeftCode(Precedence)}{(NullCoalesce ? "?" : "")}({Arguments.ArgumentsListCode()})";

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is CallExpr expr && NullCoalesce.Equals(expr.NullCoalesce) && base.Equals(_obj);

        public override int GetHashCode() => Identity.CombineHash(base.GetHashCode(), NullCoalesce);

        #endregion

    }

}
