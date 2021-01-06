using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.Runtime;
using VooDo.Utils;

namespace VooDo.AST.Expressions.Fundamentals
{

    public sealed class IndexExpr : ParametricExpr
    {

        internal IndexExpr(Expr _indexable, IEnumerable<Expr> _index, bool _nullCoalesce) : base(_indexable, _index)
        {
            if (Arguments.Count == 0)
            {
                throw new ArgumentException("Empty index", nameof(_index));
            }
            NullCoalesce = _nullCoalesce;
        }

        public bool NullCoalesce { get; }

        #region Expr

        internal sealed override Eval Evaluate(Env _env)
        {
            Eval source = Source.Evaluate(_env);
            Eval eval = Reflection.EvaluateIndexer(source, Arguments.Select(_a => _a.Evaluate(_env)).ToArray(), out Name name);
            _env.Script.HookManager.Subscribe(this, source, name);
            return eval;
        }

        internal sealed override void Assign(Env _env, Eval _value)
        {
            Eval source = Source.Evaluate(_env);
            Reflection.AssignIndexer(source, Arguments.Select(_a => _a.Evaluate(_env)).ToArray(), _value, out Name name);
            _env.Script.HookManager.Subscribe(this, source, name);
        }

        public override void Unsubscribe(HookManager _hookManager)
        {
            _hookManager.Unsubscribe(this);
            base.Unsubscribe(_hookManager);
        }

        public sealed override int Precedence => 0;

        public sealed override string Code
            => $"{Source.LeftCode(Precedence)}{(NullCoalesce ? "?" : "")}[{Arguments.ArgumentsListCode()}]";

        #endregion

        #region ASTBase

        public sealed override bool Equals(object _obj)
            => _obj is IndexExpr expr && NullCoalesce == expr.NullCoalesce && base.Equals(expr);

        public sealed override int GetHashCode()
            => Identity.CombineHash(base.GetHashCode(), NullCoalesce);

        #endregion

    }

}
