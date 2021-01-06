using System;
using System.Collections.Generic;

using VooDo.Runtime.Meta;
using VooDo.Source.Utils;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class CallExpr : ParametricExpr
    {

        internal CallExpr(Expr _callable, IEnumerable<Expr> _arguments, bool _nullCoalesce) : base(_callable, _arguments) => NullCoalesce = _nullCoalesce;

        public bool NullCoalesce { get; }

        #region Expr

        internal sealed override object Evaluate(Runtime.Env _env)
        {
            ICallable callable = Reflection.Cast<ICallable>(Source.Evaluate(_env));
            if (callable == null && NullCoalesce)
            {
                return null;
            }
            else
            {
                Arguments.Evaluate(_env, out object[] values, out Type[] types);
                return callable.Call(values, types);
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
